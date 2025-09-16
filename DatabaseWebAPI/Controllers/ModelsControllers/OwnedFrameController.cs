/*
 * Project Name:  DatabaseWebAPI
 * File Name:     OwnedFrameController.cs
 * File Function: OwnedFrame Controller 类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[ApiController]
[Route("api/ownedframe")]
[AllowAnonymous]
public class OwnedFrameController : ControllerBase
{
    private readonly OracleDbContext _db;

    public OwnedFrameController(OracleDbContext context)
    {
        _db = context;
    }

    /// <summary>
    /// 检查用户是否拥有指定头像框
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="frameColor">头像框颜色</param>
    /// <returns>1表示已拥有，0表示没有</returns>
    [HttpGet("check-if-the-frame-owned")]
    [SwaggerOperation(Summary = "检查用户是否拥有指定头像框")]
    [SwaggerResponse(200, "检查成功", typeof(int))]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<int>> CheckIfFrameOwned([FromQuery] int userId, [FromQuery] string frameColor)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(frameColor))
        {
            return BadRequest("用户ID和头像框颜色不能为空");
        }

        try
        {
            var exists = await _db.OwnedFrames.AnyAsync(of => of.UserId == userId && of.FrameColor == frameColor);
            return Ok(exists ? 1 : 0);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买头像框
    /// </summary>
    /// <param name="request">购买头像框请求</param>
    /// <returns>购买结果</returns>
    [HttpPost("buy-one-frame")]
    [SwaggerOperation(Summary = "购买头像框")]
    [SwaggerResponse(200, "购买成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(409, "头像框已拥有")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> BuyOneFrame([FromBody] BuyFrameRequest request)
    {
        if (request == null || request.UserId <= 0 || 
            string.IsNullOrWhiteSpace(request.FrameColor) || 
            string.IsNullOrWhiteSpace(request.FrameName))
        {
            return BadRequest("请求参数不能为空");
        }

        try
        {
            // 检查是否已经拥有
            var exists = await _db.OwnedFrames.AnyAsync(of => of.UserId == request.UserId && of.FrameColor == request.FrameColor);
            if (exists)
            {
                return Conflict("用户已拥有此头像框");
            }

            // 简化主键生成
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var nextId = (int)(timestamp % int.MaxValue);
            
            while (await _db.OwnedFrames.AnyAsync(of => of.OwnedFrameId == nextId))
            {
                nextId++;
            }

            var ownedFrame = new OwnedFrame
            {
                OwnedFrameId = nextId,
                UserId = request.UserId,
                FrameColor = request.FrameColor,
                FrameName = request.FrameName,
                PurchaseDate = DateTime.Now
            };

            _db.OwnedFrames.Add(ownedFrame);
            await _db.SaveChangesAsync();

            return Ok("购买成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户拥有的所有头像框
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户拥有的头像框列表</returns>
    [HttpGet("get-my-frame-list/{userId}")]
    [SwaggerOperation(Summary = "获取用户拥有的所有头像框")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> GetMyFrameList(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var ownedFrames = await _db.OwnedFrames
                .Where(of => of.UserId == userId)
                .Select(of => new { name = of.FrameName, color = of.FrameColor })
                .ToListAsync();

            return Ok(ownedFrames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}