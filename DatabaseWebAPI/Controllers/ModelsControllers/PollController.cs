/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PollController.cs
 * File Function: 投票系统控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
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

[Route("api/poll")]
[ApiController]
[SwaggerTag("投票系统 API")]
public class PollController(OracleDbContext context) : ControllerBase
{
    // 创建投票
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "创建投票", Description = "为指定帖子创建投票")]
    [SwaggerResponse(201, "创建成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> CreatePoll([FromBody] CreatePollRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PollTitle) || request.Options.Count < 2)
            {
                return BadRequest("投票标题不能为空，且至少需要2个选项");
            }

            // 检查帖子是否存在
            var post = await context.PostSet.FindAsync(request.PostId);
            if (post == null)
            {
                return BadRequest("帖子不存在");
            }

            // 检查是否已有投票
            var existingPoll = await context.Set<Poll>()
                .FirstOrDefaultAsync(p => p.PostId == request.PostId);
            if (existingPoll != null)
            {
                return BadRequest("该帖子已经有投票了");
            }

            // 创建投票
            var poll = new Poll
            {
                PostId = request.PostId,
                PollTitle = request.PollTitle.Trim(),
                PollDescription = request.PollDescription?.Trim(),
                PollType = request.PollType,
                AllowChangeVote = request.AllowChangeVote ? 1 : 0,
                ShowResults = request.ShowResults ? 1 : 0,
                DeadlineTime = request.DeadlineTime,
                IsActive = 1,
                CreatedTime = DateTime.Now
            };

            context.Set<Poll>().Add(poll);
            await context.SaveChangesAsync();

            // 创建投票选项
            var pollOptions = new List<PollOption>();
            for (int i = 0; i < request.Options.Count; i++)
            {
                var option = request.Options[i];
                if (!string.IsNullOrWhiteSpace(option.Text))
                {
                    pollOptions.Add(new PollOption
                    {
                        PollId = poll.PollId,
                        OptionText = option.Text.Trim(),
                        OptionDescription = option.Description?.Trim(),
                        SortOrder = i,
                        VoteCount = 0
                    });
                }
            }

            context.Set<PollOption>().AddRange(pollOptions);
            await context.SaveChangesAsync();

            var result = new
            {
                pollId = poll.PollId,
                postId = poll.PostId,
                title = poll.PollTitle,
                type = poll.PollType,
                optionCount = pollOptions.Count,
                createdAt = poll.CreatedTime
            };

            return CreatedAtAction(nameof(GetPollByPostId), new { postId = request.PostId }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"创建投票失败: {ex.Message}");
        }
    }

    // 根据帖子ID获取投票
    [HttpGet("post/{postId:int}")]
    [SwaggerOperation(Summary = "获取帖子的投票", Description = "根据帖子ID获取投票信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "投票不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetPollByPostId(int postId)
    {
        try
        {
            var poll = await context.Set<Poll>()
                .Include(p => p.Options.OrderBy(o => o.SortOrder))
                .FirstOrDefaultAsync(p => p.PostId == postId && p.IsActive == 1);

            if (poll == null)
            {
                return NotFound("该帖子没有投票");
            }

            // 检查用户是否已投票
            var userIdClaim = HttpContext.User.FindFirst("userId");
            var hasVoted = false;
            var userVotes = new List<int>();

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                userVotes = await context.Set<UserPollVote>()
                    .Where(v => v.PollId == poll.PollId && v.UserId == userId)
                    .Select(v => v.OptionId)
                    .ToListAsync();
                hasVoted = userVotes.Any();
            }

            var result = new
            {
                pollId = poll.PollId,
                postId = poll.PostId,
                title = poll.PollTitle,
                description = poll.PollDescription,
                type = poll.PollType == 1 ? "single" : "multiple",
                allowChangeVote = poll.AllowChangeVote == 1,
                showResults = poll.ShowResults == 1,
                deadline = poll.DeadlineTime,
                totalVotes = poll.TotalVotes,
                isExpired = poll.DeadlineTime.HasValue && poll.DeadlineTime < DateTime.Now,
                hasVoted = hasVoted,
                userVotes = userVotes,
                options = poll.Options.Select(o => new
                {
                    optionId = o.OptionId,
                    text = o.OptionText,
                    description = o.OptionDescription,
                    voteCount = o.VoteCount,
                    percentage = poll.TotalVotes > 0 ? Math.Round((double)o.VoteCount / poll.TotalVotes * 100, 1) : 0
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"获取投票失败: {ex.Message}");
        }
    }

    // 提交投票
    [HttpPost("{pollId:int}/vote")]
    [Authorize]
    [SwaggerOperation(Summary = "提交投票", Description = "用户提交投票选择")]
    [SwaggerResponse(200, "投票成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> SubmitVote(int pollId, [FromBody] SubmitVoteRequest request)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("无效的用户身份");
            }

            // 检查投票是否存在且有效
            var poll = await context.Set<Poll>().FindAsync(pollId);
            if (poll == null || poll.IsActive == 0)
            {
                return BadRequest("投票不存在或已失效");
            }

            // 检查是否已截止
            if (poll.DeadlineTime.HasValue && poll.DeadlineTime < DateTime.Now)
            {
                return BadRequest("投票已截止");
            }

            // 检查用户是否已投票
            var existingVotes = await context.Set<UserPollVote>()
                .Where(v => v.PollId == pollId && v.UserId == userId)
                .ToListAsync();

            if (existingVotes.Any() && poll.AllowChangeVote == 0)
            {
                return BadRequest("您已经投过票，且不允许修改");
            }

            // 验证选项
            var validOptions = await context.Set<PollOption>()
                .Where(o => o.PollId == pollId && request.OptionIds.Contains(o.OptionId))
                .ToListAsync();

            if (validOptions.Count != request.OptionIds.Count)
            {
                return BadRequest("包含无效的投票选项");
            }

            // 检查投票类型限制
            if (poll.PollType == 1 && request.OptionIds.Count > 1)
            {
                return BadRequest("单选投票只能选择一个选项");
            }

            // 删除旧投票（如果允许修改）
            if (existingVotes.Any())
            {
                context.Set<UserPollVote>().RemoveRange(existingVotes);
            }

            // 添加新投票
            var newVotes = request.OptionIds.Select(optionId => new UserPollVote
            {
                PollId = pollId,
                UserId = userId,
                OptionId = optionId,
                VoteTime = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
            }).ToList();

            context.Set<UserPollVote>().AddRange(newVotes);
            await context.SaveChangesAsync();

            // 返回更新后的投票结果
            var updatedPoll = await GetPollByPostId(poll.PostId);
            
            return Ok(new
            {
                message = existingVotes.Any() ? "投票修改成功" : "投票提交成功",
                pollData = updatedPoll.Value
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"提交投票失败: {ex.Message}");
        }
    }

    // 获取投票详细结果
    [HttpGet("{pollId:int}/results")]
    [SwaggerOperation(Summary = "获取投票详细结果", Description = "获取投票的详细统计结果")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "投票不存在")]
    public async Task<ActionResult<object>> GetPollResults(int pollId)
    {
        try
        {
            var poll = await context.Set<Poll>()
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.PollId == pollId);

            if (poll == null)
            {
                return NotFound("投票不存在");
            }

            // 获取投票详情
            var optionResults = new List<object>();
            
            foreach (var option in poll.Options.OrderBy(o => o.SortOrder))
            {
                var voters = await context.Set<UserPollVote>()
                    .Include(v => v.User)
                    .Where(v => v.OptionId == option.OptionId)
                    .OrderByDescending(v => v.VoteTime)
                    .Select(v => new
                    {
                        userId = v.User!.UserId,
                        userName = v.User.UserName,
                        avatarUrl = v.User.AvatarUrl,
                        voteTime = v.VoteTime,
                        level = CalculateUserLevel(v.User.ExperiencePoints)
                    })
                    .ToListAsync();

                optionResults.Add(new
                {
                    optionId = option.OptionId,
                    text = option.OptionText,
                    description = option.OptionDescription,
                    voteCount = option.VoteCount,
                    percentage = poll.TotalVotes > 0 ? Math.Round((double)option.VoteCount / poll.TotalVotes * 100, 1) : 0,
                    voters = voters
                });
            }

            return Ok(new
            {
                pollId = poll.PollId,
                title = poll.PollTitle,
                description = poll.PollDescription,
                type = poll.PollType == 1 ? "single" : "multiple",
                totalVotes = poll.TotalVotes,
                isExpired = poll.DeadlineTime.HasValue && poll.DeadlineTime < DateTime.Now,
                deadline = poll.DeadlineTime,
                options = optionResults
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"获取投票结果失败: {ex.Message}");
        }
    }

    // 删除投票（仅作者或管理员）
    [HttpDelete("{pollId:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "删除投票", Description = "删除指定的投票")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(403, "权限不足")]
    [SwaggerResponse(404, "投票不存在")]
    public async Task<ActionResult> DeletePoll(int pollId)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("无效的用户身份");
            }

            var poll = await context.Set<Poll>()
                .Include(p => p.Post)
                .FirstOrDefaultAsync(p => p.PollId == pollId);

            if (poll == null)
            {
                return NotFound("投票不存在");
            }

            // 检查权限（投票所属帖子的作者或管理员）
            var userRole = HttpContext.User.FindFirst("role")?.Value;
            if (poll.Post?.UserId != userId && userRole != "1")
            {
                return Forbid("只有帖子作者或管理员可以删除投票");
            }

            // 删除投票（级联删除选项和投票记录）
            context.Set<Poll>().Remove(poll);
            await context.SaveChangesAsync();

            return Ok(new { message = "投票删除成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"删除投票失败: {ex.Message}");
        }
    }

    // 获取用户的投票历史
    [HttpGet("user/{userId:int}/history")]
    [SwaggerOperation(Summary = "获取用户投票历史", Description = "获取指定用户的投票参与记录")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetUserVoteHistory(
        int userId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;

            var voteHistory = await context.Set<UserPollVote>()
                .Include(v => v.Poll)
                .ThenInclude(p => p!.Post)
                .Include(v => v.Option)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.VoteTime)
                .Skip(skip)
                .Take(pageSize)
                .Select(v => new
                {
                    voteId = v.VoteId,
                    pollId = v.PollId,
                    pollTitle = v.Poll!.PollTitle,
                    postId = v.Poll.PostId,
                    postTitle = v.Poll.Post!.Title,
                    selectedOption = v.Option!.OptionText,
                    voteTime = v.VoteTime,
                    isExpired = v.Poll.DeadlineTime.HasValue && v.Poll.DeadlineTime < DateTime.Now
                })
                .ToListAsync();

            var totalCount = await context.Set<UserPollVote>()
                .CountAsync(v => v.UserId == userId);

            return Ok(new
            {
                votes = voteHistory,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                hasMore = totalCount > page * pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"获取投票历史失败: {ex.Message}");
        }
    }

    #region 私有方法

    private static int CalculateUserLevel(int experiencePoints)
    {
        if (experiencePoints < 100) return 1;
        if (experiencePoints < 300) return 2;
        if (experiencePoints < 700) return 3;
        if (experiencePoints < 1500) return 4;
        if (experiencePoints < 3000) return 5;
        return Math.Min(20, experiencePoints / 1000 + 5);
    }

    #endregion
}

// 请求模型
public class CreatePollRequest
{
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }
    
    [SwaggerSchema("投票标题")]
    public string PollTitle { get; set; } = string.Empty;
    
    [SwaggerSchema("投票描述")]
    public string? PollDescription { get; set; }
    
    [SwaggerSchema("投票类型：1单选 2多选")]
    public int PollType { get; set; } = 1;
    
    [SwaggerSchema("是否允许修改投票")]
    public bool AllowChangeVote { get; set; } = true;
    
    [SwaggerSchema("是否显示结果")]
    public bool ShowResults { get; set; } = true;
    
    [SwaggerSchema("截止时间")]
    public DateTime? DeadlineTime { get; set; }
    
    [SwaggerSchema("投票选项")]
    public List<PollOptionRequest> Options { get; set; } = new();
}

public class PollOptionRequest
{
    [SwaggerSchema("选项文本")]
    public string Text { get; set; } = string.Empty;
    
    [SwaggerSchema("选项描述")]
    public string? Description { get; set; }
}

public class SubmitVoteRequest
{
    [SwaggerSchema("选择的选项ID列表")]
    public List<int> OptionIds { get; set; } = new();
}
