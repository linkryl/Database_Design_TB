/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupMessageController.cs
 * File Function: GroupMessage 控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

/// <summary>
/// 群组消息控制器
/// </summary>
[ApiController]
[Route("api/group-message")]
[SwaggerTag("群组消息相关 API")]
[AllowAnonymous] // 暂时允许匿名访问，用于开发测试
public class GroupMessageController : ControllerBase
{
    private readonly OracleDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public GroupMessageController(OracleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取群组聊天记录
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="page">页码（从1开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>消息列表</returns>
    [HttpGet("{groupId}")]
    [SwaggerOperation(Summary = "获取群组聊天记录", Description = "分页获取指定群组的聊天记录")]
    [SwaggerResponse(200, "获取成功", typeof(IEnumerable<GroupMessageDto>))]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<GroupMessageDto>>> GetGroupMessages(
        int groupId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // 验证群组是否存在
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                return NotFound("群组不存在");
            }

            var messages = await _db.GroupMessages
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .OrderByDescending(m => m.SendTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(_db.UserSet,
                    message => message.SenderId,
                    user => user.UserId,
                    (message, user) => new GroupMessageDto
                    {
                        MessageId = message.MessageId,
                        GroupId = message.GroupId,
                        SenderId = message.SenderId,
                        SenderName = user.UserName,
                        Content = message.Content,
                        MessageType = message.MessageType,
                        SendTime = message.SendTime
                        // ReplyToMessageId 字段已移除
                    })
                .ToListAsync();

            // 反转列表以按时间正序显示
            messages.Reverse();

            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送群组消息
    /// </summary>
    /// <param name="request">消息发送请求</param>
    /// <returns>发送结果</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "发送群组消息", Description = "向指定群组发送消息")]
    [SwaggerResponse(200, "发送成功", typeof(GroupMessage))]
    [SwaggerResponse(400, "请求参数无效")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<GroupMessage>> SendMessage([FromBody] SendMessageRequest request)
    {
        if (request == null || request.GroupId <= 0 || request.SenderId <= 0 || 
            string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("群组ID、发送者ID和消息内容不能为空");
        }

        try
        {
            // 调试日志
            Console.WriteLine($"[DEBUG] SendMessage - GroupId: {request.GroupId}, SenderId: {request.SenderId}");
            
            // 验证用户是否为群组成员
            var isMember = await _db.GroupMembers
                .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == request.SenderId);
            
            Console.WriteLine($"[DEBUG] IsMember check result: {isMember}");
            
            if (!isMember)
            {
                // 查看数据库中实际的群组成员
                var members = await _db.GroupMembers
                    .Where(m => m.GroupId == request.GroupId)
                    .Select(m => new { m.UserId, m.Role })
                    .ToListAsync();
                Console.WriteLine($"[DEBUG] Group {request.GroupId} actual members: {string.Join(", ", members.Select(m => $"UserId:{m.UserId},Role:{m.Role}"))}");
                
                return BadRequest("您不是该群组成员，无法发送消息");
            }

            // 生成MessageId
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var nextMessageId = (int)(timestamp % int.MaxValue);
            
            // 确保MessageId唯一
            while (await _db.GroupMessages.AnyAsync(gm => gm.MessageId == nextMessageId))
            {
                nextMessageId++;
            }

            // 创建消息
            var message = new GroupMessage
            {
                MessageId = nextMessageId,
                GroupId = request.GroupId,
                SenderId = request.SenderId,
                Content = request.Content,
                MessageType = request.MessageType,
                SendTime = DateTime.Now
                // ReplyToMessageId 暂时注释掉，因为数据库表中没有此字段
            };

            _db.GroupMessages.Add(message);

            // 更新群组最后活跃时间
            var group = await _db.Groups.FindAsync(request.GroupId);
            if (group != null)
            {
                group.LastActiveTime = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{messageId}")]
    [SwaggerOperation(Summary = "删除群组消息", Description = "删除指定的群组消息")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "消息不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DeleteMessage(int messageId, [FromQuery] int userId)
    {
        try
        {
            var message = await _db.GroupMessages.FindAsync(messageId);
            if (message == null || message.IsDeleted)
            {
                return NotFound("消息不存在");
            }

            // 只有消息发送者或群管理员可以删除消息
            var canDelete = message.SenderId == userId ||
                           await _db.GroupMembers.AnyAsync(m => 
                               m.GroupId == message.GroupId && 
                               m.UserId == userId && 
                               m.Role >= 1); // 管理员或群主

            if (!canDelete)
            {
                return BadRequest("您没有权限删除此消息");
            }

            message.IsDeleted = true;
            await _db.SaveChangesAsync();

            return Ok("删除成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

/// <summary>
/// 群组消息DTO
/// </summary>
public class GroupMessageDto
{
    public int MessageId { get; set; }
    public int GroupId { get; set; }
    public int SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? Content { get; set; }
    public int MessageType { get; set; }
    public DateTime SendTime { get; set; }
    // 暂时注释，数据库表中没有此字段
    // public int? ReplyToMessageId { get; set; }
}

/// <summary>
/// 发送消息请求模型
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// 群组ID
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public int SenderId { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 消息类型（0=文本，1=图片，2=文件）
    /// </summary>
    public int MessageType { get; set; } = 0;

    /// <summary>
    /// 回复的消息ID（可选）- 暂时注释，数据库表中没有此字段
    /// </summary>
    // public int? ReplyToMessageId { get; set; }
}