/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PostCommentController.cs
 * File Function: PostComment 控制器
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

[Route("api/post-comment")]
[ApiController]
[SwaggerTag("帖子评论表相关 API")]
public class PostCommentController(OracleDbContext context) : ControllerBase
{
    // 获取帖子评论表的所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取帖子评论表的所有数据", Description = "获取帖子评论表的所有数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<PostComment>>> GetPostComment()
    {
        try
        {
            return Ok(await context.PostCommentSet.ToListAsync());
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

    // 根据主键（ID）获取帖子评论表的数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取帖子评论表的数据", Description = "根据主键（ID）获取帖子评论表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<PostComment>> GetPostCommentByPk(int id)
    {
        try
        {
            var postComment = await context.PostCommentSet.FindAsync(id);
            if (postComment == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(postComment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据帖子 ID（POST_ID）获取帖子评论表的数据
    [HttpGet("post-{id:int}")]
    [SwaggerOperation(Summary = "根据帖子 ID（POST_ID）获取帖子评论表的数据", Description = "根据帖子 ID（POST_ID）获取帖子评论表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<PostComment>>> GetPostCommentByPostId(int id)
    {
        try
        {
            return Ok(await context.PostCommentSet
                .Where(pc => pc.PostId == id)
                .OrderByDescending(pc => pc.CommentTime)
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）删除帖子评论表的数据
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子评论表的数据", Description = "根据主键（ID）删除帖子评论表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<PostComment>> DeletePostCommentByPk(int id)
    {
        try
        {
            var postComment = await context.PostCommentSet.FindAsync(id);
            if (postComment == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            // 先删除所有相关的外键引用记录
            // 删除评论点赞记录
            var commentLikes = await context.PostCommentLikeSet
                .Where(pcl => pcl.CommentId == id)
                .ToListAsync();
            context.PostCommentLikeSet.RemoveRange(commentLikes);

            // 删除评论点踩记录
            var commentDislikes = await context.PostCommentDislikeSet
                .Where(pcd => pcd.CommentId == id)
                .ToListAsync();
            context.PostCommentDislikeSet.RemoveRange(commentDislikes);

            // 删除评论举报记录
            var commentReports = await context.PostCommentReportSet
                .Where(pcr => pcr.ReportedCommentId == id)
                .ToListAsync();
            context.PostCommentReportSet.RemoveRange(commentReports);

            // 最后删除评论本身
            context.PostCommentSet.Remove(postComment);
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 向帖子评论表添加数据项
    [HttpPost]
    [SwaggerOperation(Summary = "向帖子评论表添加数据项", Description = "向帖子评论表添加数据项（不需要提供 COMMENT_ID，因为它是由系统自动生成的）")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> PostPostComment([FromBody] PostComment postComment)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 检查用户是否被封禁
        var user = await context.UserSet.FindAsync(postComment.UserId);
        if (user == null)
        {
            return BadRequest("用户不存在");
        }
        
        if (user.Status == 0) // 0表示被封禁状态
        {
            return Forbid("您的账号已被封禁，无法评论");
        }

        // 检查帖子是否存在
        var post = await context.PostSet.FindAsync(postComment.PostId);
        if (post == null)
        {
            return BadRequest("帖子不存在");
        }

        // 如果是回复评论，检查父评论是否存在
        if (postComment.ParentCommentId.HasValue)
        {
            var parentComment = await context.PostCommentSet.FindAsync(postComment.ParentCommentId.Value);
            if (parentComment == null)
            {
                return BadRequest("被回复的评论不存在");
            }
            
            // 确保父评论属于同一篇帖子
            if (parentComment.PostId != postComment.PostId)
            {
                return BadRequest("被回复的评论不属于该帖子");
            }
        }

        context.PostCommentSet.Add(postComment);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostPostComment), new { id = postComment.CommentId }, postComment);
    }

    // 根据主键（ID）更新帖子评论表的数据
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新帖子评论表的数据", Description = "根据主键（ID）更新帖子评论表的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdatePostComment(int id, [FromBody] PostComment postComment)
    {
        if (id != postComment.CommentId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Entry(postComment).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.PostCommentSet.Any(e => e.CommentId == id))
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
}