/*
 * Project Name:  DatabaseWebAPI
 * File Name:     BarFollowController.cs
 * File Function: BarFollow 控制器 - 贴吧关注管理
 * Author:        TreeHole开发组
 * Update Date:   2025-09-14
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/bar-follow")]
[ApiController]
[SwaggerTag("贴吧关注相关 API")]
public class BarFollowController(OracleDbContext context) : ControllerBase
{
    // 关注贴吧
    [HttpPost]
    [SwaggerOperation(Summary = "关注贴吧", Description = "用户关注指定的贴吧")]
    [SwaggerResponse(201, "关注成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(409, "已经关注过该贴吧")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> FollowBar([FromBody] BarFollow barFollow)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 检查贴吧是否存在且状态正常
            var bar = await context.BarSet.FindAsync(barFollow.BarId);
            if (bar == null || bar.Status == 2)
            {
                return BadRequest("贴吧不存在或已解散");
            }

            // 检查是否已经关注
            var existingFollow = await context.BarFollowSet
                .FirstOrDefaultAsync(bf => bf.BarId == barFollow.BarId && bf.UserId == barFollow.UserId);

            if (existingFollow != null)
            {
                if (existingFollow.IsActive == 1)
                {
                    return Conflict("已经关注过该贴吧");
                }
                else
                {
                    // 重新激活关注
                    existingFollow.IsActive = 1;
                    existingFollow.FollowTime = DateTime.UtcNow;
                    existingFollow.NotificationEnabled = barFollow.NotificationEnabled;
                    await context.SaveChangesAsync();

                    // 更新贴吧关注数
                    await UpdateBarFollowedCount(barFollow.BarId);

                    return Ok("重新关注成功");
                }
            }

            // 创建新的关注记录
            barFollow.FollowTime = DateTime.UtcNow;
            barFollow.IsActive = 1;

            context.BarFollowSet.Add(barFollow);
            await context.SaveChangesAsync();

            // 更新贴吧关注数
            await UpdateBarFollowedCount(barFollow.BarId);

            return CreatedAtAction(nameof(CheckFollowStatus), 
                new { barId = barFollow.BarId, userId = barFollow.UserId }, "关注成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 取消关注贴吧
    [HttpDelete("{barId:int}/{userId:int}")]
    [SwaggerOperation(Summary = "取消关注贴吧", Description = "用户取消关注指定的贴吧")]
    [SwaggerResponse(200, "取消关注成功")]
    [SwaggerResponse(404, "未找到关注记录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UnfollowBar(int barId, int userId)
    {
        try
        {
            var barFollow = await context.BarFollowSet
                .FirstOrDefaultAsync(bf => bf.BarId == barId && bf.UserId == userId && bf.IsActive == 1);

            if (barFollow == null)
            {
                return NotFound("未找到有效的关注记录");
            }

            // 软删除：设置为非活跃状态
            barFollow.IsActive = 0;
            await context.SaveChangesAsync();

            // 更新贴吧关注数
            await UpdateBarFollowedCount(barId);

            return Ok("取消关注成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 检查关注状态
    [HttpGet("{barId:int}/{userId:int}")]
    [SwaggerOperation(Summary = "检查关注状态", Description = "检查用户是否已关注指定贴吧")]
    [SwaggerResponse(200, "已关注")]
    [SwaggerResponse(404, "未关注")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<BarFollow>> CheckFollowStatus(int barId, int userId)
    {
        try
        {
            var barFollow = await context.BarFollowSet
                .FirstOrDefaultAsync(bf => bf.BarId == barId && bf.UserId == userId && bf.IsActive == 1);

            if (barFollow == null)
            {
                return NotFound("用户未关注该贴吧");
            }

            return Ok(barFollow);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取用户关注的贴吧列表
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "获取用户关注的贴吧列表", Description = "获取指定用户关注的所有贴吧")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Bar>>> GetUserFollowedBars(int userId)
    {
        try
        {
            var followedBars = await context.BarFollowSet
                .Where(bf => bf.UserId == userId && bf.IsActive == 1)
                .Join(context.BarSet,
                    bf => bf.BarId,
                    b => b.BarId,
                    (bf, b) => new { BarFollow = bf, Bar = b })
                .Where(x => x.Bar.Status != 2) // 排除已解散的贴吧
                .OrderByDescending(x => x.BarFollow.FollowTime)
                .Select(x => x.Bar)
                .ToListAsync();

            return Ok(followedBars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取贴吧的关注者列表
    [HttpGet("bar/{barId:int}/followers")]
    [SwaggerOperation(Summary = "获取贴吧的关注者列表", Description = "获取关注指定贴吧的用户列表")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<object>>> GetBarFollowers(int barId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var followers = await context.BarFollowSet
                .Where(bf => bf.BarId == barId && bf.IsActive == 1)
                .OrderByDescending(bf => bf.FollowTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bf => new {
                    userId = bf.UserId,
                    followTime = bf.FollowTime,
                    notificationEnabled = bf.NotificationEnabled
                })
                .ToListAsync();

            return Ok(followers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取用户关注的贴吧ID列表
    [HttpGet("user/{userId:int}/bar-ids")]
    [SwaggerOperation(Summary = "获取用户关注的贴吧ID列表", Description = "只返回贴吧ID，用于快速检查关注状态")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetUserFollowedBarIds(int userId)
    {
        try
        {
            var barIds = await context.BarFollowSet
                .Where(bf => bf.UserId == userId && bf.IsActive == 1)
                .Join(context.BarSet,
                    bf => bf.BarId,
                    b => b.BarId,
                    (bf, b) => b)
                .Where(b => b.Status != 2) // 排除已解散的贴吧
                .Select(b => b.BarId)
                .ToListAsync();

            return Ok(barIds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 辅助方法：更新贴吧关注数
    private async Task UpdateBarFollowedCount(int barId)
    {
        try
        {
            var bar = await context.BarSet.FindAsync(barId);
            if (bar != null)
            {
                var followedCount = await context.BarFollowSet
                    .CountAsync(bf => bf.BarId == barId && bf.IsActive == 1);

                bar.FollowedCount = followedCount;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // 记录错误但不影响主要操作
            Console.WriteLine($"Failed to update bar followed count: {ex.Message}");
        }
    }
}

// UpdateBarStatusRequest类已在BarController.cs中定义
