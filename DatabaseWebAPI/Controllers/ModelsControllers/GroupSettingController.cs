/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupSettingController.cs
 * File Function: 群组设置管理控制器
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
/// 群组设置管理控制器
/// </summary>
[ApiController]
[Route("api/group-setting")]
[SwaggerTag("群组设置管理相关 API")]
public class GroupSettingController : ControllerBase
{
    private readonly OracleDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public GroupSettingController(OracleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 更新群组信息
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{groupId}")]
    [SwaggerOperation(Summary = "更新群组信息", Description = "更新群组名称和描述")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdateGroupInfo(int groupId, [FromBody] UpdateGroupInfoRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.GroupName))
        {
            return BadRequest("群组名称不能为空");
        }

        try
        {
            // 检查操作者权限
            var operatorMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.OperatorUserId);
            
            if (operatorMember == null || operatorMember.Role < 1)
            {
                return Forbid("您没有权限修改群组信息");
            }

            var group = await _db.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound("群组不存在");
            }

            // 更新群组信息
            group.GroupName = request.GroupName.Trim();
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                group.GroupDesc = request.Description.Trim();
            }

            await _db.SaveChangesAsync();
            return Ok("群组信息更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 设置成员权限
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="request">设置权限请求</param>
    /// <returns>设置结果</returns>
    [HttpPut("{groupId}/member-role")]
    [SwaggerOperation(Summary = "设置成员权限", Description = "设置群组成员的权限等级")]
    [SwaggerResponse(200, "设置成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "成员不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> SetMemberRole(int groupId, [FromBody] SetMemberRoleRequest request)
    {
        if (request == null || request.NewRole < 0 || request.NewRole > 2)
        {
            return BadRequest("权限等级必须在0-2之间");
        }

        try
        {
            // 检查操作者权限
            var operatorMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.OperatorUserId);
            
            if (operatorMember == null || operatorMember.Role < 2)
            {
                return Forbid("只有群主可以设置成员权限");
            }

            // 检查目标成员
            var targetMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.TargetUserId);
            
            if (targetMember == null)
            {
                return NotFound("目标用户不是群组成员");
            }

            // 不能修改自己的权限
            if (request.OperatorUserId == request.TargetUserId)
            {
                return BadRequest("不能修改自己的权限");
            }

            // 如果要设置为群主，需要将当前群主降级
            if (request.NewRole == 2)
            {
                operatorMember.Role = 1; // 原群主变为管理员
            }

            targetMember.Role = request.NewRole;
            await _db.SaveChangesAsync();

            return Ok("成员权限设置成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 禁言/解除禁言成员
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="request">禁言请求</param>
    /// <returns>操作结果</returns>
    [HttpPut("{groupId}/mute")]
    [SwaggerOperation(Summary = "禁言/解除禁言成员", Description = "对群组成员进行禁言或解除禁言")]
    [SwaggerResponse(200, "操作成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "成员不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> MuteMember(int groupId, [FromBody] MuteMemberRequest request)
    {
        try
        {
            // 检查操作者权限
            var operatorMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.OperatorUserId);
            
            if (operatorMember == null || operatorMember.Role < 1)
            {
                return Forbid("您没有权限执行此操作");
            }

            // 检查目标成员
            var targetMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.TargetUserId);
            
            if (targetMember == null)
            {
                return NotFound("目标用户不是群组成员");
            }

            // 不能对同级或更高级别的成员进行禁言
            if (targetMember.Role >= operatorMember.Role)
            {
                return Forbid("无法对同级或更高级别的成员进行此操作");
            }

            targetMember.IsMuted = request.IsMuted;
            await _db.SaveChangesAsync();

            string action = request.IsMuted ? "禁言" : "解除禁言";
            return Ok($"成员{action}成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 解散群组
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <param name="operatorUserId">操作者用户ID</param>
    /// <returns>解散结果</returns>
    [HttpDelete("{groupId}")]
    [SwaggerOperation(Summary = "解散群组", Description = "群主解散群组")]
    [SwaggerResponse(200, "解散成功")]
    [SwaggerResponse(403, "没有权限")]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DismissGroup(int groupId, [FromQuery] int operatorUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 检查操作者权限
            var operatorMember = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == operatorUserId);
            
            if (operatorMember == null || operatorMember.Role != 2)
            {
                return Forbid("只有群主可以解散群组");
            }

            var group = await _db.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound("群组不存在");
            }

            // 删除群组（级联删除会自动删除相关的成员和消息记录）
            _db.Groups.Remove(group);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("群组解散成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

/// <summary>
/// 更新群组信息请求模型
/// </summary>
public class UpdateGroupInfoRequest
{
    /// <summary>
    /// 操作者用户ID
    /// </summary>
    public int OperatorUserId { get; set; }

    /// <summary>
    /// 群组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 群组描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 设置成员权限请求模型
/// </summary>
public class SetMemberRoleRequest
{
    /// <summary>
    /// 操作者用户ID
    /// </summary>
    public int OperatorUserId { get; set; }

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public int TargetUserId { get; set; }

    /// <summary>
    /// 新的权限等级 (0: 普通成员, 1: 管理员, 2: 群主)
    /// </summary>
    public int NewRole { get; set; }
}

/// <summary>
/// 禁言成员请求模型
/// </summary>
public class MuteMemberRequest
{
    /// <summary>
    /// 操作者用户ID
    /// </summary>
    public int OperatorUserId { get; set; }

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public int TargetUserId { get; set; }

    /// <summary>
    /// 是否禁言
    /// </summary>
    public bool IsMuted { get; set; }
}