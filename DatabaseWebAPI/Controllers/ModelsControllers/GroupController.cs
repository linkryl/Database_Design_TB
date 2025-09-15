/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupController.cs
 * File Function: Group 控制器
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
/// 群组管理控制器
/// </summary>
[ApiController]
[Route("api/group")]
[SwaggerTag("群组管理相关 API")]
[AllowAnonymous] // 暂时允许匿名访问，用于开发测试
public class GroupController : ControllerBase
{
    private readonly OracleDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public GroupController(OracleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取群组列表
    /// </summary>
    /// <param name="userId">用户ID，如果提供则返回该用户所属的群组</param>
    /// <returns>群组列表</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "获取群组列表", Description = "获取群组列表，可选择性过滤用户所属群组")]
    [SwaggerResponse(200, "获取成功", typeof(IEnumerable<Group>))]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Group>>> GetGroups([FromQuery] int? userId = null)
    {
        try
        {
            IQueryable<Group> query = _db.Groups;
            
            if (userId.HasValue && userId.Value > 0)
            {
                // 获取用户所属的群组
                query = query.Where(g => _db.GroupMembers.Any(gm => gm.GroupId == g.GroupId && gm.UserId == userId.Value));
            }
            
            var groups = await query.OrderByDescending(g => g.CreateTime).ToListAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取群组信息
    /// </summary>
    /// <param name="id">群组ID</param>
    /// <returns>群组信息</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "获取群组信息", Description = "根据群组ID获取详细信息")]
    [SwaggerResponse(200, "获取成功", typeof(Group))]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Group>> GetGroup(int id)
    {
        try
        {
            var group = await _db.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound("群组不存在");
            }
            return Ok(group);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新群组
    /// </summary>
    /// <param name="request">群组创建请求</param>
    /// <returns>创建的群组信息</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "创建新群组", Description = "创建新的群组并添加初始成员")]
    [SwaggerResponse(200, "创建成功", typeof(Group))]
    [SwaggerResponse(400, "请求参数无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.GroupName) || 
            request.CreateUserId <= 0)
        {
            return BadRequest("群组名称和创建者ID不能为空");
        }

        // 确保创建者包含在成员列表中
        var memberIds = request.MemberIds ?? new List<int>();
        if (!memberIds.Contains(request.CreateUserId))
        {
            memberIds.Insert(0, request.CreateUserId); // 将创建者放在第一位
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 简化主键生成：使用当前时间戳的后几位作为ID
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var nextGroupId = (int)(timestamp % int.MaxValue);
            
            // 确保ID是正数且不重复
            while (await _db.Groups.AnyAsync(g => g.GroupId == nextGroupId))
            {
                nextGroupId++;
            }

            // 创建群组
            var group = new Group
            {
                GroupId = nextGroupId,
                GroupName = request.GroupName,
                GroupDesc = request.GroupDesc,
                CreateUserId = request.CreateUserId, // 使用创建者ID
                CreateTime = DateTime.Now,
                LastActiveTime = DateTime.Now,
                MemberCount = memberIds.Count
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            // 添加群组成员 - 简化ID生成
            var memberTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var nextMemberId = (int)(memberTimestamp % int.MaxValue);
            
            foreach (var userId in memberIds)
            {
                // 确保MemberId唯一
                while (await _db.GroupMembers.AnyAsync(gm => gm.MemberId == nextMemberId))
                {
                    nextMemberId++;
                }
                
                var member = new GroupMember
                {
                    MemberId = nextMemberId,
                    GroupId = group.GroupId,
                    UserId = userId,
                    JoinTime = DateTime.Now,
                    Role = userId == request.CreateUserId ? 2 : 0 // 创建者=群主=2，普通成员=0
                };
                _db.GroupMembers.Add(member);
                nextMemberId++; // 为下一个成员递增ID
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(group);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新群组信息
    /// </summary>
    /// <param name="id">群组ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "更新群组信息", Description = "更新群组名称或描述")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(404, "群组不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
    {
        try
        {
            var group = await _db.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound("群组不存在");
            }

            if (!string.IsNullOrWhiteSpace(request.GroupName))
            {
                group.GroupName = request.GroupName;
            }

            if (request.GroupDesc != null)
            {
                group.GroupDesc = request.GroupDesc;
            }

            await _db.SaveChangesAsync();
            return Ok("更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

/// <summary>
/// 创建群组请求模型
/// </summary>
public class CreateGroupRequest
{
    /// <summary>
    /// 群组名称
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 群组描述
    /// </summary>
    public string? GroupDesc { get; set; }

    /// <summary>
    /// 创建者用户ID
    /// </summary>
    public int CreateUserId { get; set; }

    /// <summary>
    /// 成员用户ID列表（可选，创建者会自动包含）
    /// </summary>
    public List<int>? MemberIds { get; set; }
}

/// <summary>
/// 更新群组请求模型
/// </summary>
public class UpdateGroupRequest
{
    /// <summary>
    /// 群组名称
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 群组描述
    /// </summary>
    public string? GroupDesc { get; set; }
}