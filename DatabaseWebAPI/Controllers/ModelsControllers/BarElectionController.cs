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

        // TODO：替换为真实用户身份校验（管理员或当前吧主）
        // 这里为演示先放宽权限，实际应校验 bar.OwnerId 或用户 Role==1

        var existsRunning = await context.BarElectionSet.AnyAsync(e => e.BarId == barId && e.Status == 1);
        if (existsRunning) return BadRequest("已有进行中的选举");

        var election = new BarElection
        {
            BarId = barId,
            Status = 1,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            CreatorUserId = 0
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

        // TODO：从 JWT Token 中解析用户ID（此处临时用 0 作为示例）
        var userId = 0;

        var exists = await context.BarElectionCandidateSet
            .AnyAsync(c => c.ElectionId == electionId && c.UserId == userId);
        if (exists) return BadRequest("已报名");

        var candidate = new BarElectionCandidate
        {
            ElectionId = electionId,
            UserId = userId,
            Manifesto = req.Manifesto
        };
        context.BarElectionCandidateSet.Add(candidate);
        await context.SaveChangesAsync();
        return Ok(candidate);
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

        // TODO：从 JWT Token 获取投票者ID（目前用 0 作为示例）
        var voterUserId = 0;

        var vote = new BarElectionVote
        {
            ElectionId = electionId,
            VoterUserId = voterUserId,
            CandidateUserId = req.CandidateUserId
        };
        context.BarElectionVoteSet.Add(vote);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest("请勿重复投票");
        }
        return Ok(new { message = "投票成功" });
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
}


