/*
 * File Function: 贴吧选举控制器
 * Author:        TreeHole开发组
 */

using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

// 贴吧选举相关 API：覆盖发起、报名、投票、结束与结果查询
[ApiController]
[Route("api")]
[SwaggerTag("贴吧选举相关 API")]
public class BarElectionController(OracleDbContext context) : ControllerBase
{
    // 发起选举（吧主或管理员）
    [HttpPost("bar/{barId:int}/elections")]
    [Authorize]
    [SwaggerOperation(Summary = "发起选举", Description = "仅吧主或管理员可发起")]
    public async Task<IActionResult> CreateElection(int barId, [FromBody] CreateElectionRequest req)
    {
        // 校验时间
        if (req.EndTime <= req.StartTime) return BadRequest("结束时间必须晚于开始时间");
        var bar = await context.BarSet.FindAsync(barId);
        if (bar == null) return NotFound("贴吧不存在");

        // 获取当前用户ID
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized("未登录");
        }

        // 检查权限：必须是吧主或管理员
        var user = await context.UserSet.FindAsync(currentUserId.Value);
        if (user == null) return Unauthorized("用户不存在");
        
        bool isAdmin = user.Role == 1;
        bool isBarOwner = bar.OwnerId == currentUserId.Value;
        
        if (!isAdmin && !isBarOwner)
        {
            return Forbid("只有吧主或管理员可以发起选举");
        }

        var existsRunning = await context.BarElectionSet.AnyAsync(e => e.BarId == barId && e.Status == 1);
        if (existsRunning) return BadRequest("已有进行中的选举");

        var election = new BarElection
        {
            BarId = barId,
            Status = 1,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            CreatorUserId = currentUserId.Value
        };
        context.BarElectionSet.Add(election);
        await context.SaveChangesAsync();
        return Ok(election);
    }

    // 查询当前选举
    [HttpGet("bar/{barId:int}/elections/current")]
    public async Task<IActionResult> GetCurrentElection(int barId)
    {
        var election = await context.BarElectionSet
            .Where(e => e.BarId == barId && e.Status == 1)
            .OrderByDescending(e => e.ElectionId)
            .FirstOrDefaultAsync();
        if (election == null) return NotFound("当前无进行中的选举");
        return Ok(election);
    }

    // 报名参选（建议限制为该吧已关注用户）
    [HttpPost("bar-elections/{electionId:int}/candidates")]
    [Authorize]
    public async Task<IActionResult> JoinElection(int electionId, [FromBody] JoinElectionRequest req)
    {
        var election = await context.BarElectionSet.FindAsync(electionId);
        if (election == null || election.Status != 1) return BadRequest("选举不存在或未进行中");

        // 简化：省略校验是否关注该吧、是否被封禁，需要时可查询 BarFollow 和 User.Status

        // 获取当前用户ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("未登录");
        }

        // 验证用户是否存在
        var user = await context.UserSet.FindAsync(userId.Value);
        if (user == null)
        {
            return BadRequest("用户不存在，请重新登录");
        }

        // 检查用户是否被封禁
        if (user.Status == 0)
        {
            return Forbid("您的账号已被封禁，无法参选");
        }

        // 检查是否已经报名
        var exists = await context.BarElectionCandidateSet
            .AnyAsync(c => c.ElectionId == electionId && c.UserId == userId.Value);
        if (exists) return BadRequest("您已经报名过了");

        // 检查选举是否还在进行中
        if (DateTime.UtcNow > election.EndTime)
        {
            return BadRequest("选举已结束，无法报名");
        }

        var candidate = new BarElectionCandidate
        {
            ElectionId = electionId,
            UserId = userId.Value,
            Manifesto = req.Manifesto,
            CreateTime = DateTime.UtcNow
        };
        
        try
        {
            context.BarElectionCandidateSet.Add(candidate);
            await context.SaveChangesAsync();
            return Ok(new { message = "报名成功", candidateId = candidate.CandidateId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"报名失败: {ex.Message}. 内部异常: {ex.InnerException?.Message}");
        }
    }

    // 候选人列表
    [HttpGet("bar-elections/{electionId:int}/candidates")]
    public async Task<IActionResult> GetCandidates(int electionId)
    {
        var list = await context.BarElectionCandidateSet
            .Where(c => c.ElectionId == electionId)
            .ToListAsync();
        return Ok(list);
    }

    // 投票（建议限制为该吧已关注用户，每人每场选举仅一次）
    [HttpPost("bar-elections/{electionId:int}/vote")]
    [Authorize]
    public async Task<IActionResult> Vote(int electionId, [FromBody] VoteElectionRequest req)
    {
        var election = await context.BarElectionSet.FindAsync(electionId);
        if (election == null || election.Status != 1) return BadRequest("选举不存在或未进行中");

        var validCandidate = await context.BarElectionCandidateSet
            .AnyAsync(c => c.ElectionId == electionId && c.UserId == req.CandidateUserId);
        if (!validCandidate) return BadRequest("候选人不存在");

        // 获取当前用户ID
        var voterUserId = GetCurrentUserId();
        if (voterUserId == null)
        {
            return Unauthorized("未登录");
        }

        // 验证投票者是否存在
        var voter = await context.UserSet.FindAsync(voterUserId.Value);
        if (voter == null)
        {
            return BadRequest("投票者不存在，请重新登录");
        }

        // 检查是否被封禁
        if (voter.Status == 0)
        {
            return Forbid("您的账号已被封禁，无法投票");
        }

        // 检查选举是否还在进行中
        if (DateTime.UtcNow > election.EndTime)
        {
            return BadRequest("选举已结束，无法投票");
        }

        // 检查是否已经投票过
        var existingVote = await context.BarElectionVoteSet
            .FirstOrDefaultAsync(v => v.ElectionId == electionId && v.VoterUserId == voterUserId.Value);
        if (existingVote != null)
        {
            return BadRequest("您已经投过票了，每人只能投一票");
        }

        var vote = new BarElectionVote
        {
            ElectionId = electionId,
            VoterUserId = voterUserId.Value,
            CandidateUserId = req.CandidateUserId,
            VoteTime = DateTime.UtcNow
        };
        
        try
        {
            context.BarElectionVoteSet.Add(vote);
            await context.SaveChangesAsync();
            return Ok(new { message = "投票成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"投票失败: {ex.Message}. 内部异常: {ex.InnerException?.Message}");
        }
    }

    // 结束选举并更新吧主（取最高票，平票策略可扩展）
    [HttpPost("bar-elections/{electionId:int}/close")]
    [Authorize]
    public async Task<IActionResult> CloseElection(int electionId)
    {
        var election = await context.BarElectionSet.FindAsync(electionId);
        if (election == null) return NotFound("选举不存在");

        var bar = await context.BarSet.FindAsync(election.BarId);
        if (bar == null) return NotFound("贴吧不存在");

        if (election.Status == 2) return BadRequest("选举已结束");

        // 统计票数
        var grouped = await context.BarElectionVoteSet
            .Where(v => v.ElectionId == electionId)
            .GroupBy(v => v.CandidateUserId)
            .Select(g => new { CandidateUserId = g.Key, Votes = g.Count() })
            .OrderByDescending(x => x.Votes)
            .ThenBy(x => x.CandidateUserId)
            .ToListAsync();

        if (grouped.Count == 0)
        {
            election.Status = 2;
            await context.SaveChangesAsync();
            return Ok(new { message = "无人投票，选举结束", winner = (int?)null });
        }

        var winnerUserId = grouped.First().CandidateUserId;

        // 更新吧主
        bar.OwnerId = winnerUserId;
        election.Status = 2;
        await context.SaveChangesAsync();
        return Ok(new { message = "选举结束", winner = winnerUserId });
    }

    // 查看结果
    [HttpGet("bar-elections/{electionId:int}/results")]
    public async Task<IActionResult> GetResults(int electionId)
    {
        var grouped = await context.BarElectionVoteSet
            .Where(v => v.ElectionId == electionId)
            .GroupBy(v => v.CandidateUserId)
            .Select(g => new { CandidateUserId = g.Key, Votes = g.Count() })
            .OrderByDescending(x => x.Votes)
            .ThenBy(x => x.CandidateUserId)
            .ToListAsync();
        return Ok(grouped);
    }

    // 调试API：检查当前用户信息
    [HttpGet("debug/current-user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserDebugInfo()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Ok(new { message = "无法获取用户ID", claims = User.Claims.Select(c => new { c.Type, c.Value }) });
            }

            var user = await context.UserSet.FindAsync(currentUserId.Value);
            return Ok(new {
                userId = currentUserId.Value,
                userExists = user != null,
                userStatus = user?.Status,
                userName = user?.UserName,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"调试失败: {ex.Message}");
        }
    }

    // 获取当前登录用户ID的私有方法
    private int? GetCurrentUserId()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}


