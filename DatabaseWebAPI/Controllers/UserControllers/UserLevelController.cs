/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserLevelController.cs
 * File Function: 用户等级系统控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatabaseWebAPI.Services;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DatabaseWebAPI.Controllers.UserControllers;

[Route("api/user-level")]
[ApiController]
[SwaggerTag("用户等级系统 API")]
public class UserLevelController : ControllerBase
{
    private readonly IUserLevelService _userLevelService;
    private readonly ILogger<UserLevelController> _logger;

    public UserLevelController(IUserLevelService userLevelService, ILogger<UserLevelController> logger)
    {
        _userLevelService = userLevelService;
        _logger = logger;
    }

    // 获取用户等级信息
    [HttpGet("{userId:int}")]
    [SwaggerOperation(Summary = "获取用户等级信息", Description = "获取指定用户的等级、经验值、勋章等信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<UserLevelInfo>> GetUserLevelInfo(int userId)
    {
        try
        {
            var levelInfo = await _userLevelService.GetUserLevelInfoAsync(userId);
            return Ok(levelInfo);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户等级信息失败: UserId={UserId}", userId);
            return StatusCode(500, "获取用户等级信息失败");
        }
    }

    // 用户签到
    [HttpPost("check-in")]
    [Authorize]
    [SwaggerOperation(Summary = "用户签到", Description = "用户每日签到，获得经验值奖励")]
    [SwaggerResponse(200, "签到成功")]
    [SwaggerResponse(400, "今日已签到")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<CheckInResult>> CheckIn()
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("无效的用户身份");
            }

            var result = await _userLevelService.CheckInAsync(userId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户签到失败");
            return StatusCode(500, "签到失败，请稍后重试");
        }
    }

    // 获取用户勋章
    [HttpGet("{userId:int}/badges")]
    [SwaggerOperation(Summary = "获取用户勋章", Description = "获取用户已获得的所有勋章")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<UserBadgeDto>>> GetUserBadges(int userId)
    {
        try
        {
            var badges = await _userLevelService.GetUserBadgesAsync(userId);
            return Ok(badges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户勋章失败: UserId={UserId}", userId);
            return StatusCode(500, "获取用户勋章失败");
        }
    }

    // 获取用户任务
    [HttpGet("{userId:int}/tasks")]
    [SwaggerOperation(Summary = "获取用户任务", Description = "获取用户的所有任务及完成进度")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<UserTaskDto>>> GetUserTasks(int userId)
    {
        try
        {
            var tasks = await _userLevelService.GetUserTasksAsync(userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户任务失败: UserId={UserId}", userId);
            return StatusCode(500, "获取用户任务失败");
        }
    }

    // 完成任务
    [HttpPost("tasks/{taskId:int}/complete")]
    [Authorize]
    [SwaggerOperation(Summary = "完成任务", Description = "标记指定任务为完成状态")]
    [SwaggerResponse(200, "任务完成")]
    [SwaggerResponse(400, "任务已完成或不存在")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> CompleteTask(int taskId)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("无效的用户身份");
            }

            var success = await _userLevelService.CompleteTaskAsync(userId, taskId);
            
            if (success)
            {
                return Ok(new { message = "任务完成，奖励已发放" });
            }
            else
            {
                return BadRequest("任务已完成或不存在");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "完成任务失败: TaskId={TaskId}", taskId);
            return StatusCode(500, "完成任务失败");
        }
    }

    // 添加经验值（管理员功能）
    [HttpPost("{userId:int}/add-experience")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "添加经验值", Description = "管理员为用户添加经验值")]
    [SwaggerResponse(200, "添加成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(403, "权限不足")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> AddExperience(int userId, [FromBody] AddExperienceRequest request)
    {
        try
        {
            if (request.Experience <= 0)
            {
                return BadRequest("经验值必须大于0");
            }

            var success = await _userLevelService.AddExperienceAsync(
                userId, 
                request.Experience, 
                "ADMIN_GRANT", 
                request.Reason ?? "管理员添加"
            );

            if (success)
            {
                return Ok(new { message = "经验值添加成功" });
            }
            else
            {
                return BadRequest("用户不存在或添加失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加经验值失败: UserId={UserId}", userId);
            return StatusCode(500, "添加经验值失败");
        }
    }

    // 获取所有等级配置
    [HttpGet("levels")]
    [SwaggerOperation(Summary = "获取所有等级配置", Description = "获取系统中所有等级的配置信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<UserLevelConfig>>> GetAllLevels()
    {
        try
        {
            var levels = await _userLevelService.GetAllLevelsAsync();
            return Ok(levels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取等级配置失败");
            return StatusCode(500, "获取等级配置失败");
        }
    }

    // 获取用户排名信息
    [HttpGet("{userId:int}/rank")]
    [SwaggerOperation(Summary = "获取用户排名", Description = "获取用户在全站的经验值排名")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<UserRankInfo>> GetUserRank(int userId)
    {
        try
        {
            var rankInfo = await _userLevelService.GetUserRankAsync(userId);
            return Ok(rankInfo);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户排名失败: UserId={UserId}", userId);
            return StatusCode(500, "获取用户排名失败");
        }
    }

    // 获取经验值排行榜
    [HttpGet("leaderboard")]
    [SwaggerOperation(Summary = "获取经验值排行榜", Description = "获取经验值前N名的用户列表")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<object>>> GetLeaderboard([FromQuery] int limit = 50)
    {
        try
        {
            // 这里可以添加缓存来提高性能
            var leaderboard = await _userLevelService.GetLeaderboardAsync(limit);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取排行榜失败");
            return StatusCode(500, "获取排行榜失败");
        }
    }

    // 获取用户签到历史
    [HttpGet("{userId:int}/check-in-history")]
    [SwaggerOperation(Summary = "获取用户签到历史", Description = "获取用户最近的签到记录")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<object>>> GetCheckInHistory(int userId, [FromQuery] int days = 30)
    {
        try
        {
            var history = await _userLevelService.GetCheckInHistoryAsync(userId, days);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取签到历史失败: UserId={UserId}", userId);
            return StatusCode(500, "获取签到历史失败");
        }
    }

    // 获取用户经验值获取记录
    [HttpGet("{userId:int}/experience-log")]
    [SwaggerOperation(Summary = "获取经验值记录", Description = "获取用户经验值变化记录")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<object>>> GetExperienceLog(
        int userId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var log = await _userLevelService.GetExperienceLogAsync(userId, page, pageSize);
            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取经验值记录失败: UserId={UserId}", userId);
            return StatusCode(500, "获取经验值记录失败");
        }
    }
}

// 请求模型
public class AddExperienceRequest
{
    [SwaggerSchema("经验值数量")]
    public int Experience { get; set; }
    
    [SwaggerSchema("添加原因")]
    public string? Reason { get; set; }
}
