/*
 * Project Name:  DatabaseWebAPI
 * File Name:     ExperienceController.cs
 * File Function: 用户经验值和等级管理控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-08-01
 * License:       Creative Commons Attribution 4.0 International License
 */

using System;
using System.Threading.Tasks;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/experience")]
[ApiController]
[SwaggerTag("用户经验值和等级相关 API")]
public class ExperienceController(OracleDbContext context) : ControllerBase
{
    // 获取用户等级信息
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "获取用户等级信息", Description = "根据用户ID获取当前等级、经验值和进度信息")]
    [SwaggerResponse(200, "获取成功", typeof(LevelInfoResponse))]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<LevelInfoResponse>> GetUserLevelInfo(int userId)
    {
        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"未找到ID为 {userId} 的用户");
            }

            int level = ExperienceUtils.CalculateLevel(user.ExperiencePoints);
            int expToNextLevel = ExperienceUtils.GetExpToNextLevel(level, user.ExperiencePoints);
            double progress = ExperienceUtils.GetProgressPercentage(level, user.ExperiencePoints);

            var response = new LevelInfoResponse
            {
                Level = level,
                CurrentExp = user.ExperiencePoints,
                ExpToNextLevel = expToNextLevel,
                ProgressPercentage = Math.Round(progress, 2) // 保留两位小数
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器内部错误: {ex.Message}");
        }
    }

    // 增加用户经验值
    [HttpPost("add")]
    [SwaggerOperation(Summary = "增加用户经验值", Description = "为用户增加指定数量的经验值")]
    [SwaggerResponse(200, "经验值增加成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> AddExperiencePoints([FromBody] ExperiencePointsRequest request)
    {
        if (request.Points <= 0)
        {
            return BadRequest("经验值必须大于0");
        }

        try
        {
            var user = await context.UserSet.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound($"未找到ID为 {request.UserId} 的用户");
            }

            // 获取增加前的等级
            int oldLevel = ExperienceUtils.CalculateLevel(user.ExperiencePoints);

            // 增加经验值
            user.ExperiencePoints += request.Points;

            // 获取增加后的等级
            int newLevel = ExperienceUtils.CalculateLevel(user.ExperiencePoints);

            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok(new
            {
                Message = "经验值增加成功",
                AddedPoints = request.Points,
                TotalPoints = user.ExperiencePoints,
                OldLevel = oldLevel,
                NewLevel = newLevel,
                LevelUp = newLevel > oldLevel
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器内部错误: {ex.Message}");
        }
    }
}