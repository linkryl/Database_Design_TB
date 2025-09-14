/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PostController.cs
 * File Function: Post 控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/post")]
[ApiController]
[SwaggerTag("帖子表相关 API")]
public class PostController(OracleDbContext context) : ControllerBase
{
    // 获取帖子表的所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取帖子表的所有数据", Description = "获取帖子表的所有数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Post>>> GetPost()
    {
        try
        {
            return Ok(await context.PostSet.ToListAsync());
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

    // 根据主键（ID）获取帖子表的数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取帖子表的数据", Description = "根据主键（ID）获取帖子表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Post>> GetPostByPk(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）删除帖子表的数据
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子表的数据", Description = "根据主键（ID）删除帖子表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<Post>> DeletePostByPk(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            context.PostSet.Remove(post);
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 向帖子表添加数据项
    [HttpPost]
    [SwaggerOperation(Summary = "向帖子表添加数据项", Description = "向帖子表添加数据项（不需要提供 POST_ID，因为它是由系统自动生成的）")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> PostPost([FromBody] Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 检查用户是否被封禁（同学添加的功能）
            var user = await context.UserSet.FindAsync(post.UserId);
            if (user == null)
            {
                return BadRequest("用户不存在");
            }
            
            if (user.Status == 0) // 0表示被封禁状态
            {
                return Forbid("您的账号已被封禁，无法发帖");
            }

            // 设置默认值（我们添加的功能）
            // AlsoInTreehole字段默认为0，无需额外检查

            context.PostSet.Add(post);
            await context.SaveChangesAsync();
            
            // 重新查询帖子以避免循环引用问题
            var createdPost = await context.PostSet
                .Where(p => p.PostId == post.PostId)
                .Select(p => new
                {
                    p.PostId,
                    p.UserId,
                    p.CategoryId,
                    p.Title,
                    p.Content,
                    p.CreationDate,
                    p.UpdateDate,
                    p.IsSticky,
                    p.LikeCount,
                    p.DislikeCount,
                    p.FavoriteCount,
                    p.CommentCount,
                    p.ImageUrl
                })
                .FirstOrDefaultAsync();
            
            return CreatedAtAction(nameof(PostPost), new { id = post.PostId }, createdPost);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据用户 ID（UserId）获取帖子ID列表
    [HttpGet("user-ids/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户 ID（UserId）获取帖子ID列表", Description = "根据用户 ID（UserId）获取该用户发布的所有帖子ID")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetPostIdsByUserId(int userId)
    {
        try
        {
            var postIds = await context.PostSet
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreationDate)
                .Select(p => p.PostId)
                .ToListAsync();
            return Ok(postIds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）更新帖子表的数据
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新帖子表的数据", Description = "根据主键（ID）更新帖子表的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdatePost(int id, [FromBody] Post post)
    {
        if (id != post.PostId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Entry(post).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.PostSet.Any(e => e.PostId == id))
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

    // 根据帖子分类（CategoryId）筛选帖子表的主键（ID）
    [HttpPost("filter-by-category")]
    [SwaggerOperation(Summary = "根据帖子分类（CategoryId）筛选帖子表的主键（ID）", Description = "根据帖子分类（CategoryId）筛选帖子表的主键（ID）")]
    [SwaggerResponse(200, "筛选成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<IEnumerable<int>>> FilterPostIdByCategoryId([FromBody] List<int> categoryIds)
    {
        if (categoryIds.Count == 0)
        {
            return BadRequest("Category ID list cannot be null or empty.");
        }

        try
        {
            var postIds = await context.PostSet
                .Where(post => categoryIds.Contains(post.CategoryId))
                .OrderByDescending(post => post.IsSticky)
                .ThenByDescending(post => post.CreationDate)
                .Select(post => post.PostId)
                .ToListAsync();
            return Ok(postIds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("latest-ids")]
    [SwaggerOperation(Summary = "按创建时间倒序获取最新帖子的ID列表", Description = "只返回按创建时间排序的最新帖子主键ID")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetLatestPostIds([FromQuery] int count = 10)
    {
        try
        {
            var postIds = await context.PostSet
                .OrderByDescending(post => post.CreationDate)
                .Take(count)
                .Select(post => post.PostId)
                .ToListAsync();
            return Ok(postIds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 注释：贴吧相关API暂时禁用，等数据库BAR_ID字段确认后再启用
    // 避免Git冲突和编译错误
}