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

    // 根据主键（ID）删除帖子表的数据（管理员用）
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子表的数据", Description = "根据主键（ID）删除帖子表的数据（管理员功能）")]
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

    // 用户删除自己的帖子
    [HttpDelete("{id:int}/user/{userId:int}")]
    [SwaggerOperation(Summary = "用户删除自己的帖子", Description = "用户只能删除自己发布的帖子")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(403, "无权限删除")]
    [SwaggerResponse(404, "帖子不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> DeleteMyPost(int id, int userId)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound("帖子不存在");
            }

            // 检查是否是该用户的帖子
            if (post.UserId != userId)
            {
                return Forbid("您只能删除自己的帖子");
            }

            // 使用原生SQL绕过触发器冲突，按正确顺序删除
            using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // 临时禁用相关触发器，避免ORA-04091错误
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_FAVORITE_COUNTS DISABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_LIKE_COUNTS DISABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_C_COUNT DISABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_DISLIKE_COUNT DISABLE");
                
                // 删除帖子（级联删除会自动删除相关记录）
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM POST WHERE POST_ID = {0} AND USER_ID = {1}", 
                    id, userId);
                
                // 重新启用触发器
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_FAVORITE_COUNTS ENABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_LIKE_COUNTS ENABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_C_COUNT ENABLE");
                await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_DISLIKE_COUNT ENABLE");
                
                await transaction.CommitAsync();
                
                return Ok(new { message = "帖子删除成功", postId = id });
            }
            catch (Exception ex)
            {
                // 发生错误时确保重新启用触发器
                try
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_FAVORITE_COUNTS ENABLE");
                    await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_LIKE_COUNTS ENABLE");
                    await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_C_COUNT ENABLE");
                    await context.Database.ExecuteSqlRawAsync("ALTER TRIGGER DECREMENT_POST_DISLIKE_COUNT ENABLE");
                }
                catch
                {
                    // 忽略重新启用触发器时的错误
                }
                
                await transaction.RollbackAsync();
                throw new Exception($"删除失败: {ex.Message}", ex);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"删除帖子失败: {ex.Message}. 内部异常: {ex.InnerException?.Message}");
        }
    }

    // 软删除帖子（备用方案）- 标记删除而不是物理删除
    [HttpPut("{id:int}/user/{userId:int}/soft-delete")]
    [SwaggerOperation(Summary = "软删除自己的帖子", Description = "将帖子标记为删除状态，不进行物理删除")]
    [SwaggerResponse(200, "标记删除成功")]
    [SwaggerResponse(403, "无权限删除")]
    [SwaggerResponse(404, "帖子不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> SoftDeleteMyPost(int id, int userId)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound("帖子不存在");
            }

            // 检查是否是该用户的帖子
            if (post.UserId != userId)
            {
                return Forbid("您只能删除自己的帖子");
            }

            // 使用软删除：在标题前添加[DELETED]标记
            post.Title = "[DELETED]" + post.Title;
            post.Content = "[DELETED]该帖子已被作者删除";
            post.UpdateDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            
            return Ok(new { message = "帖子删除成功", postId = id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"软删除帖子失败: {ex.Message}");
        }
    }

    // 向帖子表添加数据项 - 支持分区功能
    [HttpPost]
    [SwaggerOperation(Summary = "向帖子表添加数据项", Description = "向帖子表添加数据项，支持树洞和贴吧分区")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> PostPost([FromBody] dynamic requestData)
    {
        try
        {
            // 解析请求数据
            var jsonElement = (System.Text.Json.JsonElement)requestData;
            
            // 检查是否为高级发帖请求
            bool isAdvancedPost = jsonElement.TryGetProperty("publishType", out var publishTypeElement);
            
            if (isAdvancedPost)
            {
                // 处理高级发帖请求
                return await ProcessAdvancedPost(jsonElement);
            }
            else
            {
                // 处理普通发帖请求
                return await ProcessNormalPost(jsonElement);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProcessAdvancedPost(System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            var userId = jsonElement.GetProperty("userId").GetInt32();
            var categoryId = jsonElement.GetProperty("categoryId").GetInt32();
            var publishType = jsonElement.GetProperty("publishType").GetString();
            var title = jsonElement.GetProperty("title").GetString();
            var content = jsonElement.GetProperty("content").GetString();
            
            int? barId = null;
            bool alsoInTreehole = false;
            
            if (jsonElement.TryGetProperty("barId", out var barIdElement) && !barIdElement.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
            {
                barId = barIdElement.GetInt32();
            }
            
            if (jsonElement.TryGetProperty("alsoInTreehole", out var alsoInTreeholeElement))
            {
                alsoInTreehole = alsoInTreeholeElement.GetBoolean();
            }

            // 检查用户是否被封禁
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("用户不存在");
            }
            
            if (user.Status == 0)
            {
                return Forbid("您的账号已被封禁，无法发帖");
            }

            var createdPosts = new List<object>();
            var currentTime = DateTime.UtcNow;

            if (publishType == "treehole")
            {
                // 只发布到树洞社区
                var treeholePost = new Post
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Title = title,
                    Content = content, // 树洞帖子内容不添加标记
                    CreationDate = currentTime,
                    UpdateDate = currentTime,
                    IsSticky = 0,
                    LikeCount = 0,
                    DislikeCount = 0,
                    FavoriteCount = 0,
                    CommentCount = 0,
                    ImageUrl = null
                };

                context.PostSet.Add(treeholePost);
                await context.SaveChangesAsync();
                
                createdPosts.Add(new { PostId = treeholePost.PostId, Location = "treehole" });
            }
            else if (publishType == "bar" && barId.HasValue)
            {
                // 检查贴吧是否存在
                var bar = await context.BarSet.FindAsync(barId.Value);
                if (bar == null || bar.Status != 0)
                {
                    return BadRequest("贴吧不存在或已解散");
                }

                // 创建贴吧帖子（内容前添加贴吧标记）
                var barPost = new Post
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Title = title,
                    Content = $"[BAR:{barId.Value}]{content}", // 添加贴吧标记
                    CreationDate = currentTime,
                    UpdateDate = currentTime,
                    IsSticky = 0,
                    LikeCount = 0,
                    DislikeCount = 0,
                    FavoriteCount = 0,
                    CommentCount = 0,
                    ImageUrl = null
                };

                context.PostSet.Add(barPost);
                await context.SaveChangesAsync();
                
                createdPosts.Add(new { PostId = barPost.PostId, Location = "bar", BarId = barId.Value });

                // 如果选择了跨发布，还要创建一个树洞帖子
                if (alsoInTreehole)
                {
                    var treeholePost = new Post
                    {
                        UserId = userId,
                        CategoryId = categoryId,
                        Title = title,
                        Content = content, // 树洞版本不添加标记
                        CreationDate = currentTime,
                        UpdateDate = currentTime,
                        IsSticky = 0,
                        LikeCount = 0,
                        DislikeCount = 0,
                        FavoriteCount = 0,
                        CommentCount = 0,
                        ImageUrl = null
                    };

                    context.PostSet.Add(treeholePost);
                    await context.SaveChangesAsync();
                    
                    createdPosts.Add(new { PostId = treeholePost.PostId, Location = "treehole" });
                }

                // 更新贴吧帖子数
                bar.PostCount++;
                bar.UpdateDate = currentTime;
                await context.SaveChangesAsync();
            }

            return Ok(new { 
                Success = true,
                Posts = createdPosts,
                Message = $"发帖成功，共创建{createdPosts.Count}篇帖子"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Advanced post error: {ex.Message}");
        }
    }

    private async Task<IActionResult> ProcessNormalPost(System.Text.Json.JsonElement jsonElement)
    {
        try
        {
            var post = System.Text.Json.JsonSerializer.Deserialize<Post>(jsonElement.GetRawText());
            
            if (post == null)
            {
                return BadRequest("Invalid post data");
            }

            // 检查用户是否被封禁
            var user = await context.UserSet.FindAsync(post.UserId);
            if (user == null)
            {
                return BadRequest("用户不存在");
            }
            
            if (user.Status == 0)
            {
                return Forbid("您的账号已被封禁，无法发帖");
            }

            context.PostSet.Add(post);
            await context.SaveChangesAsync();
            
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
            return StatusCode(500, $"Normal post error: {ex.Message}");
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
    [SwaggerOperation(Summary = "按创建时间倒序获取最新帖子的ID列表", Description = "只返回按创建时间排序的最新帖子主键ID，现在只返回树洞帖子，过滤用户举报的帖子")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetLatestPostIds([FromQuery] int count = 10, [FromQuery] int? userId = null)
    {
        try
        {
            var query = context.PostSet
                .Where(post => !post.Content.StartsWith("[BAR:") && !post.Title.StartsWith("[DELETED]")); // 实现分区：只显示树洞帖子，过滤已删除帖子
            
            // 如果指定了用户ID，过滤掉该用户举报过的帖子
            if (userId.HasValue)
            {
                var reportedPostIds = context.PostReportSet
                    .Where(report => report.ReporterId == userId.Value)
                    .Select(report => report.ReportedPostId)
                    .ToList();
                
                query = query.Where(post => !reportedPostIds.Contains(post.PostId));
            }
                
            var postIds = await query
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

    // 获取树洞社区帖子ID列表（不包含贴吧帖子）
    [HttpGet("treehole-ids")]
    [SwaggerOperation(Summary = "获取树洞社区帖子ID列表", Description = "获取只属于树洞社区的帖子ID，不包含贴吧帖子，过滤用户举报的帖子")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetTreeholePostIds([FromQuery] int count = 20, [FromQuery] int? userId = null)
    {
        try
        {
            var query = context.PostSet
                .Where(post => !post.Content.StartsWith("[BAR:") && !post.Title.StartsWith("[DELETED]")); // 不包含贴吧标记的帖子，过滤已删除帖子
            
            // 如果指定了用户ID，过滤掉该用户举报过的帖子
            if (userId.HasValue)
            {
                var reportedPostIds = context.PostReportSet
                    .Where(report => report.ReporterId == userId.Value)
                    .Select(report => report.ReportedPostId)
                    .ToList();
                
                query = query.Where(post => !reportedPostIds.Contains(post.PostId));
            }
                
            var postIds = await query
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

    // 获取指定贴吧的帖子ID列表
    [HttpGet("bar-ids/{barId:int}")]
    [SwaggerOperation(Summary = "获取指定贴吧的帖子ID列表", Description = "获取属于指定贴吧的帖子ID，过滤用户举报的帖子")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "贴吧不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<int>>> GetBarPostIds(int barId, [FromQuery] int count = 20, [FromQuery] int? userId = null)
    {
        try
        {
            // 检查贴吧是否存在
            var barExists = await context.BarSet.AnyAsync(b => b.BarId == barId && b.Status == 0);
            if (!barExists)
            {
                return NotFound("贴吧不存在或已解散");
            }

            var barPrefix = $"[BAR:{barId}]";
            var query = context.PostSet
                .Where(post => post.Content.StartsWith(barPrefix) && !post.Title.StartsWith("[DELETED]")); // 过滤已删除帖子
            
            // 如果指定了用户ID，过滤掉该用户举报过的帖子
            if (userId.HasValue)
            {
                var reportedPostIds = context.PostReportSet
                    .Where(report => report.ReporterId == userId.Value)
                    .Select(report => report.ReportedPostId)
                    .ToList();
                
                query = query.Where(post => !reportedPostIds.Contains(post.PostId));
            }
                
            var postIds = await query
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

    // 高级创建帖子方法，支持地点和跨发布功能
    [HttpPost("advanced")]
    [SwaggerOperation(Summary = "高级发帖功能", Description = "支持选择发布位置和跨发布功能的发帖接口")]
    [SwaggerResponse(201, "发帖成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> CreateAdvancedPost([FromBody] AdvancedPostRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 检查用户是否被封禁
            var user = await context.UserSet.FindAsync(request.UserId);
            if (user == null)
            {
                return BadRequest("用户不存在");
            }
            
            if (user.Status == 0)
            {
                return Forbid("您的账号已被封禁，无法发帖");
            }

            var createdPosts = new List<object>();
            var currentTime = DateTime.UtcNow;

            if (request.PublishType == "treehole")
            {
                // 只发布到树洞社区
                var treeholePost = new Post
                {
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    Title = request.Title,
                    Content = request.Content, // 树洞帖子内容不添加标记
                    CreationDate = currentTime,
                    UpdateDate = currentTime,
                    IsSticky = 0,
                    LikeCount = 0,
                    DislikeCount = 0,
                    FavoriteCount = 0,
                    CommentCount = 0,
                    ImageUrl = request.ImageUrl
                };

                context.PostSet.Add(treeholePost);
                await context.SaveChangesAsync();
                
                createdPosts.Add(new { PostId = treeholePost.PostId, Location = "treehole" });
            }
            else if (request.PublishType == "bar" && request.BarId.HasValue)
            {
                // 检查贴吧是否存在
                var bar = await context.BarSet.FindAsync(request.BarId.Value);
                if (bar == null || bar.Status != 0)
                {
                    return BadRequest("贴吧不存在或已解散");
                }

                // 创建贴吧帖子（内容前添加贴吧标记）
                var barPost = new Post
                {
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    Title = request.Title,
                    Content = $"[BAR:{request.BarId.Value}]{request.Content}", // 添加贴吧标记
                    CreationDate = currentTime,
                    UpdateDate = currentTime,
                    IsSticky = 0,
                    LikeCount = 0,
                    DislikeCount = 0,
                    FavoriteCount = 0,
                    CommentCount = 0,
                    ImageUrl = request.ImageUrl
                };

                context.PostSet.Add(barPost);
                await context.SaveChangesAsync();
                
                createdPosts.Add(new { PostId = barPost.PostId, Location = "bar", BarId = request.BarId.Value });

                // 如果选择了跨发布，还要创建一个树洞帖子
                if (request.AlsoInTreehole)
                {
                    var treeholePost = new Post
                    {
                        UserId = request.UserId,
                        CategoryId = request.CategoryId,
                        Title = request.Title,
                        Content = request.Content, // 树洞版本不添加标记
                        CreationDate = currentTime,
                        UpdateDate = currentTime,
                        IsSticky = 0,
                        LikeCount = 0,
                        DislikeCount = 0,
                        FavoriteCount = 0,
                        CommentCount = 0,
                        ImageUrl = request.ImageUrl
                    };

                    context.PostSet.Add(treeholePost);
                    await context.SaveChangesAsync();
                    
                    createdPosts.Add(new { PostId = treeholePost.PostId, Location = "treehole" });
                }

                // 更新贴吧帖子数
                bar.PostCount++;
                bar.UpdateDate = currentTime;
                await context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("发布类型无效或缺少必要参数");
            }

            return CreatedAtAction(nameof(CreateAdvancedPost), new { }, new { 
                Success = true,
                Posts = createdPosts,
                Message = $"发帖成功，共创建{createdPosts.Count}篇帖子"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取帖子的位置信息
    [HttpGet("{id:int}/location")]
    [SwaggerOperation(Summary = "获取帖子位置信息", Description = "获取帖子是否属于贴吧以及贴吧信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "帖子不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetPostLocation(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound("帖子不存在");
            }

            // 检查是否为贴吧帖子
            if (post.Content.StartsWith("[BAR:"))
            {
                // 提取贴吧ID
                var barPrefix = "[BAR:";
                var startIndex = barPrefix.Length;
                var endIndex = post.Content.IndexOf(']', startIndex);
                
                if (endIndex > startIndex && int.TryParse(post.Content.Substring(startIndex, endIndex - startIndex), out int barId))
                {
                    var bar = await context.BarSet.FindAsync(barId);
                    return Ok(new
                    {
                        Location = "bar",
                        BarId = barId,
                        BarName = bar?.BarName ?? "未知贴吧",
                        BarExists = bar != null && bar.Status == 0
                    });
                }
            }

            return Ok(new { Location = "treehole" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

// 高级发帖请求模型
public class AdvancedPostRequest
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string PublishType { get; set; } = string.Empty; // "treehole" 或 "bar"
    public int? BarId { get; set; } // 仅当PublishType为"bar"时需要
    public bool AlsoInTreehole { get; set; } = false; // 是否同时在树洞显示
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}