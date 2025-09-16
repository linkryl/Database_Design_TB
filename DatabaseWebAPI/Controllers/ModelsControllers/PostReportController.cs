/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PostReportController.cs
 * File Function: PostReport 控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/post-report")]
[ApiController]
[SwaggerTag("帖子举报表相关 API")]
public class PostReportController(OracleDbContext context) : ControllerBase
{
    // 获取帖子举报表的所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取帖子举报表的所有数据", Description = "获取帖子举报表的所有数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<PostReport>>> GetPostReport()
    {
        try
        {
            return Ok(await context.PostReportSet.ToListAsync());
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Database update error: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）获取帖子举报表的数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取帖子举报表的数据", Description = "根据主键（ID）获取帖子举报表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<PostReport>> GetPostReportByPk(int id)
    {
        try
        {
            var postReport = await context.PostReportSet.FindAsync(id);
            if (postReport == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(postReport);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）删除帖子举报表的数据
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子举报表的数据", Description = "根据主键（ID）删除帖子举报表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<PostReport>> DeletePostReportByPk(int id)
    {
        try
        {
            var postReport = await context.PostReportSet.FindAsync(id);
            if (postReport == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            context.PostReportSet.Remove(postReport);
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 向帖子举报表添加数据项
    [HttpPost]
    [SwaggerOperation(Summary = "向帖子举报表添加数据项", Description = "向帖子举报表添加数据项（不需要提供 POST_REPORT_ID，因为它是由系统自动生成的）")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> PostPostReport([FromBody] dynamic requestData)
    {
        try
        {
            var jsonElement = (System.Text.Json.JsonElement)requestData;
            
            // 提取数据
            var reporterId = jsonElement.GetProperty("reporterId").GetInt32();
            var reportedUserId = jsonElement.GetProperty("reportedUserId").GetInt32();
            var reportedPostId = jsonElement.GetProperty("reportedPostId").GetInt32();
            var reportReason = jsonElement.GetProperty("reportReason").GetString();
            var reportTimeStr = jsonElement.GetProperty("reportTime").GetString();
            var status = jsonElement.TryGetProperty("status", out var statusElement) ? statusElement.GetInt32() : 0;

            // 验证被举报的帖子是否存在
            var post = await context.PostSet.FindAsync(reportedPostId);
            if (post == null)
            {
                return BadRequest("被举报的帖子不存在");
            }

            // 验证举报者是否存在
            var reporter = await context.UserSet.FindAsync(reporterId);
            if (reporter == null)
            {
                return BadRequest("举报者不存在");
            }

            // 验证被举报者是否存在
            var reportedUser = await context.UserSet.FindAsync(reportedUserId);
            if (reportedUser == null)
            {
                return BadRequest("被举报者不存在");
            }

            // 检查是否已经举报过
            var existingReport = await context.PostReportSet
                .FirstOrDefaultAsync(r => r.ReporterId == reporterId && r.ReportedPostId == reportedPostId);
            if (existingReport != null)
            {
                return BadRequest("您已经举报过这篇帖子了");
            }

            // 解析时间
            DateTime reportTime;
            if (!DateTime.TryParse(reportTimeStr, out reportTime))
            {
                reportTime = DateTime.UtcNow;
            }

            // 创建举报记录
            var postReport = new PostReport
            {
                ReporterId = reporterId,
                ReportedUserId = reportedUserId,
                ReportedPostId = reportedPostId,
                ReportReason = reportReason,
                ReportTime = reportTime,
                Status = status
            };

            context.PostReportSet.Add(postReport);
            await context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(PostPostReport), new { id = postReport.PostReportId }, 
                new { message = "举报提交成功", reportId = postReport.PostReportId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"举报失败: {ex.Message}. 内部异常: {ex.InnerException?.Message}");
        }
    }

    // 根据主键（ID）更新帖子举报表的数据
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新帖子举报表的数据", Description = "根据主键（ID）更新帖子举报表的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdatePostReport(int id, [FromBody] PostReport postReport)
    {
        if (id != postReport.PostReportId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Entry(postReport).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.PostReportSet.Any(e => e.PostReportId == id))
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return Ok($"Data with ID: {id} has been updated successfully.");
    }

    // 获取帖子举报管理数据
    [HttpGet("management")]
    [SwaggerOperation(Summary = "获取帖子举报管理数据", Description = "获取帖子举报管理数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<object>>> GetPostReportManagement()
    {
        try
        {
            return Ok(await context.PostReportSet
                .OrderByDescending(p => p.ReportTime)
                .Select(p => new
                {
                    p.PostReportId,
                    p.ReporterId,
                    ReporterName = p.Reporter!.UserName,
                    p.ReportedUserId,
                    ReportedUserName = p.ReportedUser!.UserName,
                    p.ReportedPostId,
                    p.ReportedPost!.Title,
                    p.ReportReason,
                    p.ReportTime,
                    p.Status
                })
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}