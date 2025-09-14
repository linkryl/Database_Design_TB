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

        // 检查用户是否被封禁
        var user = await context.UserSet.FindAsync(post.UserId);
        if (user == null)
        {
            return BadRequest("用户不存在");
        }
        
        if (user.Status == 0) // 0表示被封禁状态
        {
            return Forbid("您的账号已被封禁，无法发帖");
        }

        context.PostSet.Add(post);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostPost), new { id = post.PostId }, post);
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

    // 管理员删除帖子接口
    [HttpDelete("admin/{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "管理员删除帖子", Description = "只有管理员可以删除帖子")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(401, "未授权")]
    [SwaggerResponse(403, "权限不足")]
    [SwaggerResponse(404, "帖子不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> AdminDeletePost(int id)
    {
        try
        {
            // 调试：打印所有Claims
            Console.WriteLine("=== 管理员删除帖子调试信息 ===");
            Console.WriteLine($"所有Claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
            
            // 获取当前用户信息 - 尝试多种方式
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                             User.FindFirst("userId") ?? 
                             User.FindFirst("sub");
            Console.WriteLine($"找到的userIdClaim: {userIdClaim?.Type} = {userIdClaim?.Value}");
            
            int userId;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                Console.WriteLine($"直接解析用户ID: {userId}");
            }
            else
            {
                Console.WriteLine("无法直接解析用户ID，尝试其他方式");
                return Unauthorized("无法获取用户信息");
            }

            // 检查用户是否为管理员
            var user = await context.UserSet.FindAsync(userId);
            if (user == null || user.Role != 1) // 1表示管理员角色
            {
                return Forbid("只有管理员可以删除帖子");
            }

            // 查找帖子
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"帖子ID {id} 不存在");
            }

            // 删除帖子（级联删除相关数据）
            context.PostSet.Remove(post);
            await context.SaveChangesAsync();

            return Ok(new { message = $"帖子ID {id} 已成功删除", deletedPost = new { id = post.PostId, title = post.Title } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}