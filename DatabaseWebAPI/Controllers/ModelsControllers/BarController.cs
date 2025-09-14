/*
 * Project Name:  DatabaseWebAPI
 * File Name:     BarController.cs
 * File Function: Bar 控制器 - 贴吧管理
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

[Route("api/bar")]
[ApiController]
[SwaggerTag("贴吧管理相关 API")]
public class BarController(OracleDbContext context) : ControllerBase
{
    // 获取所有贴吧（按创建时间倒序）
    [HttpGet]
    [SwaggerOperation(Summary = "获取所有贴吧", Description = "获取所有贴吧信息，按创建时间倒序")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Bar>>> GetAllBars([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var bars = await context.BarSet
                .Where(b => b.Status == 0) // 只获取正常状态的贴吧
                .OrderByDescending(b => b.FollowedCount) // 按关注数排序
                .ThenByDescending(b => b.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(bars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据ID获取贴吧详情
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据ID获取贴吧详情", Description = "根据主键ID获取贴吧详细信息")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Bar>> GetBarById(int id)
    {
        try
        {
            var bar = await context.BarSet.FindAsync(id);
            if (bar == null)
            {
                return NotFound($"No bar found for ID: {id}");
            }

            return Ok(bar);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据吧主ID获取贴吧列表
    [HttpGet("owner/{ownerId:int}")]
    [SwaggerOperation(Summary = "根据吧主ID获取贴吧列表", Description = "获取指定用户创建的所有贴吧")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Bar>>> GetBarsByOwnerId(int ownerId)
    {
        try
        {
            var bars = await context.BarSet
                .Where(b => b.OwnerId == ownerId && b.Status != 2) // 排除已解散的贴吧
                .OrderByDescending(b => b.CreationDate)
                .ToListAsync();

            return Ok(bars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 创建贴吧
    [HttpPost]
    [SwaggerOperation(Summary = "创建贴吧", Description = "创建新的贴吧")]
    [SwaggerResponse(201, "创建贴吧成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> CreateBar([FromBody] Bar bar)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 检查贴吧名称是否已存在
            var existingBar = await context.BarSet
                .FirstOrDefaultAsync(b => b.BarName == bar.BarName && b.Status != 2);
            
            if (existingBar != null)
            {
                return BadRequest("贴吧名称已存在，请选择其他名称");
            }

            // 设置创建时间和初始状态
            bar.CreationDate = DateTime.UtcNow;
            bar.UpdateDate = DateTime.UtcNow;
            bar.Status = 0; // 正常状态
            bar.FollowedCount = 0;
            bar.PostCount = 0;

            context.BarSet.Add(bar);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBarById), new { id = bar.BarId }, bar);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 更新贴吧信息
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "更新贴吧信息", Description = "更新贴吧的基本信息（仅吧主可操作）")]
    [SwaggerResponse(200, "更新贴吧成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(403, "无权限操作")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdateBar(int id, [FromBody] Bar bar)
    {
        if (id != bar.BarId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingBar = await context.BarSet.FindAsync(id);
            if (existingBar == null)
            {
                return NotFound($"No bar found for ID: {id}");
            }

            // 检查重名（排除自己）
            var duplicateName = await context.BarSet
                .AnyAsync(b => b.BarName == bar.BarName && b.BarId != id && b.Status != 2);
            
            if (duplicateName)
            {
                return BadRequest("贴吧名称已存在，请选择其他名称");
            }

            // 更新允许修改的字段
            existingBar.BarName = bar.BarName;
            existingBar.Description = bar.Description;
            existingBar.AvatarUrl = bar.AvatarUrl;
            existingBar.CoverUrl = bar.CoverUrl;
            existingBar.Rules = bar.Rules;
            existingBar.Tags = bar.Tags;
            existingBar.UpdateDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return Ok("贴吧信息更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 更新贴吧状态（归档/解散）
    [HttpPut("{id:int}/status")]
    [SwaggerOperation(Summary = "更新贴吧状态", Description = "更新贴吧状态：0=正常，1=归档，2=已解散")]
    [SwaggerResponse(200, "状态更新成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(403, "无权限操作")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdateBarStatus(int id, [FromBody] UpdateBarStatusRequest request)
    {
        try
        {
            var bar = await context.BarSet.FindAsync(id);
            if (bar == null)
            {
                return NotFound($"No bar found for ID: {id}");
            }

            bar.Status = request.Status;
            bar.UpdateDate = DateTime.UtcNow;

            await context.SaveChangesAsync();

            string statusText = request.Status switch
            {
                0 => "正常",
                1 => "归档",
                2 => "已解散",
                _ => "未知状态"
            };

            return Ok($"贴吧状态已更新为：{statusText}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 搜索贴吧
    [HttpGet("search")]
    [SwaggerOperation(Summary = "搜索贴吧", Description = "根据关键词搜索贴吧名称和描述")]
    [SwaggerResponse(200, "搜索成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Bar>>> SearchBars([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("搜索关键词不能为空");
        }

        try
        {
            var bars = await context.BarSet
                .Where(b => b.Status == 0 && 
                           (b.BarName.Contains(keyword) || 
                            (b.Description != null && b.Description.Contains(keyword)) ||
                            (b.Tags != null && b.Tags.Contains(keyword))))
                .OrderByDescending(b => b.FollowedCount)
                .ThenByDescending(b => b.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(bars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取热门贴吧
    [HttpGet("popular")]
    [SwaggerOperation(Summary = "获取热门贴吧", Description = "按关注数获取热门贴吧")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Bar>>> GetPopularBars([FromQuery] int count = 10)
    {
        try
        {
            var bars = await context.BarSet
                .Where(b => b.Status == 0)
                .OrderByDescending(b => b.FollowedCount)
                .ThenByDescending(b => b.PostCount)
                .Take(count)
                .ToListAsync();

            return Ok(bars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

// 辅助请求类
public class UpdateBarStatusRequest
{
    public int Status { get; set; }
}
