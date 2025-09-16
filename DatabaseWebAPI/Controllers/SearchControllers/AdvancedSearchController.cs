/*
 * Project Name:  DatabaseWebAPI
 * File Name:     AdvancedSearchController.cs
 * File Function: 高级搜索控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.RegularExpressions;

namespace DatabaseWebAPI.Controllers.SearchControllers;

[Route("api/search")]
[ApiController]
[SwaggerTag("高级搜索功能 API")]
public class AdvancedSearchController(OracleDbContext context) : ControllerBase
{
    // 综合搜索
    [HttpGet("comprehensive")]
    [SwaggerOperation(Summary = "综合搜索", Description = "搜索帖子、用户、贴吧等内容")]
    [SwaggerResponse(200, "搜索成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> ComprehensiveSearch(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "relevance") // relevance, time, popularity
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return BadRequest("搜索关键词至少需要2个字符");
            }

            // 清理和预处理关键词
            var cleanKeyword = CleanSearchKeyword(keyword);
            var searchWords = cleanKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 记录搜索历史
            await RecordSearchHistory(keyword, 4); // 4表示综合搜索

            // 搜索帖子
            var posts = await SearchPosts(searchWords, page, pageSize / 2, sortBy);
            
            // 搜索用户
            var users = await SearchUsers(searchWords, 1, 5);
            
            // 搜索贴吧
            var bars = await SearchBars(searchWords, 1, 5);

            var result = new
            {
                keyword = keyword,
                total = posts.TotalCount + users.Count + bars.Count,
                posts = new
                {
                    items = posts.Items,
                    totalCount = posts.TotalCount,
                    hasMore = posts.HasMore
                },
                users = users,
                bars = bars,
                searchTime = DateTime.Now
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"搜索失败: {ex.Message}");
        }
    }

    // 帖子搜索
    [HttpGet("posts")]
    [SwaggerOperation(Summary = "搜索帖子", Description = "根据关键词搜索帖子内容")]
    [SwaggerResponse(200, "搜索成功")]
    public async Task<ActionResult<object>> SearchPosts(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "relevance",
        [FromQuery] int? categoryId = null,
        [FromQuery] int? barId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var cleanKeyword = CleanSearchKeyword(keyword);
            var searchWords = cleanKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            await RecordSearchHistory(keyword, 1); // 1表示帖子搜索
            
            var result = await SearchPosts(searchWords, page, pageSize, sortBy, categoryId, barId, startDate, endDate);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"搜索帖子失败: {ex.Message}");
        }
    }

    // 用户搜索
    [HttpGet("users")]
    [SwaggerOperation(Summary = "搜索用户", Description = "根据用户名搜索用户")]
    public async Task<ActionResult<List<object>>> SearchUsers(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var cleanKeyword = CleanSearchKeyword(keyword);
            var searchWords = cleanKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            await RecordSearchHistory(keyword, 2); // 2表示用户搜索
            
            var result = await SearchUsers(searchWords, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"搜索用户失败: {ex.Message}");
        }
    }

    // 贴吧搜索
    [HttpGet("bars")]
    [SwaggerOperation(Summary = "搜索贴吧", Description = "根据贴吧名称搜索贴吧")]
    public async Task<ActionResult<List<object>>> SearchBars(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var cleanKeyword = CleanSearchKeyword(keyword);
            var searchWords = cleanKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            await RecordSearchHistory(keyword, 3); // 3表示贴吧搜索
            
            var result = await SearchBars(searchWords, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"搜索贴吧失败: {ex.Message}");
        }
    }

    // 获取搜索建议
    [HttpGet("suggestions")]
    [SwaggerOperation(Summary = "获取搜索建议", Description = "根据输入获取搜索建议词")]
    public async Task<ActionResult<List<string>>> GetSearchSuggestions([FromQuery] string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 1)
            {
                return Ok(new List<string>());
            }

            var suggestions = new List<string>();

            // 从搜索建议表获取
            var dbSuggestions = await context.Set<SearchSuggestion>()
                .Where(s => s.Keyword.Contains(input) && s.IsActive == 1)
                .OrderByDescending(s => s.Weight)
                .Select(s => s.Suggestion)
                .Take(5)
                .ToListAsync();

            suggestions.AddRange(dbSuggestions);

            // 从热门搜索词获取
            var hotKeywords = await context.Set<HotSearchKeyword>()
                .Where(k => k.Keyword.Contains(input))
                .OrderByDescending(k => k.SearchCount)
                .Select(k => k.Keyword)
                .Take(5)
                .ToListAsync();

            suggestions.AddRange(hotKeywords);

            // 去重并限制数量
            var uniqueSuggestions = suggestions.Distinct().Take(10).ToList();

            return Ok(uniqueSuggestions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"获取搜索建议失败: {ex.Message}");
        }
    }

    // 获取热门搜索词
    [HttpGet("hot-keywords")]
    [SwaggerOperation(Summary = "获取热门搜索词", Description = "获取当前热门的搜索关键词")]
    public async Task<ActionResult<List<object>>> GetHotKeywords([FromQuery] int limit = 10)
    {
        try
        {
            var hotKeywords = await context.Set<HotSearchKeyword>()
                .OrderByDescending(k => k.SearchCount)
                .ThenByDescending(k => k.LastSearchTime)
                .Take(limit)
                .Select(k => new
                {
                    keyword = k.Keyword,
                    searchCount = k.SearchCount,
                    trend = "hot" // 可以根据增长趋势计算
                })
                .ToListAsync();

            return Ok(hotKeywords);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"获取热门搜索词失败: {ex.Message}");
        }
    }

    #region 私有方法

    private async Task<(List<object> Items, int TotalCount, bool HasMore)> SearchPosts(
        string[] searchWords, 
        int page, 
        int pageSize, 
        string sortBy,
        int? categoryId = null,
        int? barId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = context.PostSet.AsQueryable();

        // 构建搜索条件
        foreach (var word in searchWords)
        {
            query = query.Where(p => p.Title.Contains(word) || p.Content.Contains(word));
        }

        // 添加筛选条件
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (barId.HasValue)
        {
            // 注意：这里假设Post表有BarId字段，根据实际情况调整
            // query = query.Where(p => p.BarId == barId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreationDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.CreationDate <= endDate.Value);
        }

        // 排序
        query = sortBy switch
        {
            "time" => query.OrderByDescending(p => p.CreationDate),
            "popularity" => query.OrderByDescending(p => p.LikeCount),
            "comments" => query.OrderByDescending(p => p.CommentCount),
            _ => query.OrderByDescending(p => p.CreationDate) // 默认按时间排序
        };

        var totalCount = await query.CountAsync();
        var hasMore = totalCount > page * pageSize;

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.User)
            .Include(p => p.PostCategory)
            .Select(p => new
            {
                postId = p.PostId,
                title = p.Title,
                content = p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
                creationDate = p.CreationDate,
                likeCount = p.LikeCount,
                commentCount = p.CommentCount,
                user = new
                {
                    userId = p.User!.UserId,
                    userName = p.User.UserName,
                    avatarUrl = p.User.AvatarUrl
                },
                category = new
                {
                    categoryId = p.PostCategory!.CategoryId,
                    category = p.PostCategory.Category
                },
                // 添加搜索高亮信息
                highlights = GetHighlights(p.Title + " " + p.Content, searchWords)
            })
            .ToListAsync();

        return (posts.Cast<object>().ToList(), totalCount, hasMore);
    }

    private async Task<List<object>> SearchUsers(string[] searchWords, int page, int pageSize)
    {
        var query = context.UserSet.Where(u => u.Status == 1); // 只搜索正常状态用户

        foreach (var word in searchWords)
        {
            query = query.Where(u => u.UserName.Contains(word));
        }

        var users = await query
            .OrderByDescending(u => u.ExperiencePoints) // 按经验值排序
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                userId = u.UserId,
                userName = u.UserName,
                avatarUrl = u.AvatarUrl,
                profile = u.Profile,
                experiencePoints = u.ExperiencePoints,
                followedCount = u.FollowedCount,
                registrationDate = u.RegistrationDate
            })
            .ToListAsync();

        return users.Cast<object>().ToList();
    }

    private async Task<List<object>> SearchBars(string[] searchWords, int page, int pageSize)
    {
        var query = context.Set<Bar>().Where(b => b.Status == 1); // 只搜索正常状态贴吧

        foreach (var word in searchWords)
        {
            query = query.Where(b => b.BarName.Contains(word) || 
                                   (b.Description != null && b.Description.Contains(word)));
        }

        var bars = await query
            .OrderByDescending(b => b.FollowedCount) // 按关注数排序
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                barId = b.BarId,
                barName = b.BarName,
                description = b.Description,
                avatarUrl = b.AvatarUrl,
                followedCount = b.FollowedCount,
                postCount = b.PostCount,
                creationDate = b.CreationDate
            })
            .ToListAsync();

        return bars.Cast<object>().ToList();
    }

    private string CleanSearchKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return string.Empty;

        // 移除特殊字符，保留中文、英文、数字和空格
        var cleaned = Regex.Replace(keyword.Trim(), @"[^\w\s\u4e00-\u9fa5]", " ");
        
        // 合并多个空格为单个空格
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        
        return cleaned.Trim();
    }

    private List<string> GetHighlights(string text, string[] searchWords)
    {
        var highlights = new List<string>();
        
        foreach (var word in searchWords)
        {
            var index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var start = Math.Max(0, index - 20);
                var length = Math.Min(text.Length - start, 60);
                var highlight = text.Substring(start, length);
                
                // 简单高亮标记
                highlight = highlight.Replace(word, $"<mark>{word}</mark>", StringComparison.OrdinalIgnoreCase);
                highlights.Add(highlight);
            }
        }
        
        return highlights;
    }

    private async Task RecordSearchHistory(string keyword, int searchType)
    {
        try
        {
            // 更新热门搜索词
            var hotKeyword = await context.Set<HotSearchKeyword>()
                .FirstOrDefaultAsync(h => h.Keyword == keyword);

            if (hotKeyword != null)
            {
                hotKeyword.SearchCount++;
                hotKeyword.LastSearchTime = DateTime.Now;
            }
            else
            {
                context.Set<HotSearchKeyword>().Add(new HotSearchKeyword
                {
                    Keyword = keyword,
                    SearchCount = 1,
                    LastSearchTime = DateTime.Now,
                    CreatedTime = DateTime.Now
                });
            }

            // 记录搜索历史（如果用户已登录）
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Set<SearchHistory>().Add(new SearchHistory
                {
                    UserId = userId,
                    SearchKeyword = keyword,
                    SearchType = searchType,
                    SearchTime = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 记录搜索历史失败不应该影响搜索功能
            Console.WriteLine($"记录搜索历史失败: {ex.Message}");
        }
    }

    #endregion
}

// 搜索相关模型类
public class SearchHistory
{
    public int SearchId { get; set; }
    public int? UserId { get; set; }
    public string SearchKeyword { get; set; } = string.Empty;
    public int SearchType { get; set; }
    public int ResultCount { get; set; }
    public DateTime SearchTime { get; set; }
    public string? IpAddress { get; set; }
}

public class HotSearchKeyword
{
    public int KeywordId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public DateTime LastSearchTime { get; set; }
    public DateTime CreatedTime { get; set; }
}

public class SearchSuggestion
{
    public int SuggestionId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int IsActive { get; set; }
    public DateTime CreatedTime { get; set; }
}
