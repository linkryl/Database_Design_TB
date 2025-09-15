/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupMemberController.cs
 * File Function: GroupMember 控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

/// <summary>
/// 群组成员管理控制器
/// </summary>
[ApiController]
[Route("api/group-member")]
[SwaggerTag("群组成员管理相关 API")]
public class GroupMemberController : ControllerBase
{
    private readonly OracleDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public GroupMemberController(OracleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取群组成员列表
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns>成员列表</returns>
    [HttpGet("{groupId}")]
    [SwaggerOperation(Summary = "获取群组成员列表", Description = "获取指定群组的所有成员信息")]
    [SwaggerResponse(200, "获取成功", typeof(IEnumerable<GroupMemberDto>))]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetGroupMembers(int groupId)
    {
        try
        {
            // 验证群组是否存在
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                return NotFound("群组不存在");
            }

            var members = await _db.GroupMembers
                .Where(m => m.GroupId == groupId)
                .Join(_db.UserSet,
                    member => member.UserId,
                    user => user.UserId,
                    (member, user) => new GroupMemberDto
                    {
                        MemberId = member.MemberId,
                        GroupId = member.GroupId,
                        UserId = member.UserId,
                        UserName = user.UserName,
                        JoinTime = member.JoinTime,
                        Role = member.Role,
                        IsMuted = member.IsMuted
                    })
                .OrderByDescending(m => m.Role) // 先显示群主和管理员
                .ThenBy(m => m.JoinTime)
                .ToListAsync();

            return Ok(members);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 加入群组
    /// </summary>
    /// <param name="request">加入群组请求</param>
    /// <returns>加入结果</returns>
    [HttpPost("join")]
    [SwaggerOperation(Summary = "加入群组", Description = "用户加入指定群组")]
    [SwaggerResponse(200, "加入成功")]
    [SwaggerResponse(400, "请求参数无效")]
    [SwaggerResponse(409, "已是群组成员")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupRequest request)
    {
        if (request == null || request.GroupId <= 0 || request.UserId <= 0)
        {
            return BadRequest("群组ID和用户ID不能为空");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 检查是否已经是成员
            var existingMember = await _db.GroupMembers
                .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == request.UserId);
            
            if (existingMember)
            {
                return Conflict("您已经是该群组成员");
            }

            // 检查群组是否存在
            var group = await _db.Groups.FindAsync(request.GroupId);
            if (group == null)
            {
                return NotFound("群组不存在");
            }

            // 添加成员
            var member = new GroupMember
            {
                GroupId = request.GroupId,
                UserId = request.UserId,
                JoinTime = DateTime.Now,
                Role = 0 // 普通成员
            };

            _db.GroupMembers.Add(member);

            // 更新群组成员数量
            group.MemberCount++;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("加入群组成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 退出群组
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>退出结果</returns>
    [HttpDelete("{groupId}/leave")]
    [SwaggerOperation(Summary = "退出群组", Description = "用户退出指定群组")]
    [SwaggerResponse(200, "退出成功")]
    [SwaggerResponse(404, "不是群组成员")]
    [SwaggerResponse(400, "群主不能退出群组")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> LeaveGroup(int groupId, [FromQuery] int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var member = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            
            if (member == null)
            {
                return NotFound("您不是该群组成员");
            }

            // 群主不能直接退出，需要先转让群主
            if (member.Role == 2)
            {
                return BadRequest("群主不能退出群组，请先转让群主权限");
            }

            // 删除成员记录
            _db.GroupMembers.Remove(member);

            // 更新群组成员数量
            var group = await _db.Groups.FindAsync(groupId);
            if (group != null)
            {
                group.MemberCount--;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("退出群组成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 踢出群组成员
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="targetUserId">被踢出的用户ID</param>
    /// <param name="operatorUserId">操作者用户ID</param>
    /// <returns>踢出结果</returns>
    [HttpDelete("{groupId}/kick")]
    [SwaggerOperation(Summary = "踢出群组成员", Description = "管理员或群主踢出指定成员")]
    [SwaggerResponse(200, "踢出成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "成员不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> KickMember(int groupId, [FromQuery] int targetUserId, [FromQuery] int operatorUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 检查操作者权限
            var operatorMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == operatorUserId);
            
            if (operatorMember == null || operatorMember.Role < 1)
            {
                return Forbid("您没有权限执行此操作");
            }

            // 检查目标成员
            var targetMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);
            
            if (targetMember == null)
            {
                return NotFound("目标用户不是群组成员");
            }

            // 不能踢出群主，管理员不能踢出同级或更高级别的成员
            if (targetMember.Role >= operatorMember.Role)
            {
                return Forbid("无法踢出同级或更高级别的成员");
            }

            // 删除成员记录
            _db.GroupMembers.Remove(targetMember);

            // 更新群组成员数量
            var group = await _db.Groups.FindAsync(groupId);
            if (group != null)
            {
                group.MemberCount--;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("踢出成员成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

/// <summary>
/// 群组成员DTO
/// </summary>
public class GroupMemberDto
{
    public int MemberId { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime JoinTime { get; set; }
    public int Role { get; set; }
    public bool IsMuted { get; set; }
}

/// <summary>
/// 加入群组请求模型
/// </summary>
public class JoinGroupRequest
{
    /// <summary>
    /// 群组ID
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }
}