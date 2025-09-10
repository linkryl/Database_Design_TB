/*
 * Project Name:  DatabaseWebAPI
 * File Name:     SearchController.cs
 * File Function: 搜索控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.SearchControllers;

[ApiController]
[Route("api/search")]
[SwaggerTag("搜索相关 API")]
public class SearchController(OracleDbContext context) : ControllerBase
{
    // 获取帖子的搜索数据
    [HttpGet("post")]
    [SwaggerOperation(Summary = "获取帖子的搜索数据", Description = "获取帖子的搜索数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<PostSearchRequest>>> GetPostSearchData([FromQuery] string? keyword)  
    {
        try
        {
            var query = context.PostSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => 
                    p.Title.Contains(keyword) || 
                    p.Content.Contains(keyword));
            }

            var result = await query
                .Select(p => new PostSearchRequest
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Content = p.Content
                })
                .ToListAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取帖子评论的搜索数据
    [HttpGet("post-comment")]
    [SwaggerOperation(Summary = "获取帖子评论的搜索数据", Description = "获取帖子评论的搜索数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<PostCommentSearchRequest>>> GetPostCommentSearchData()
    {
        try
        {
            return Ok(await context.PostCommentSet.Select(p =>
                new PostCommentSearchRequest
                {
                    PostId = p.PostId,
                    Title = p.Post!.Title,
                    Content = p.Content
                }).ToListAsync());
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

}