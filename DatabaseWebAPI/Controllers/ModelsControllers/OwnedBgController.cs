/*
 * Project Name:  DatabaseWebAPI
 * File Name:     OwnedBgController.cs
 * File Function: OwnedBg Controller 类
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
[Route("api/ownedbg")]
[AllowAnonymous]
public class OwnedBgController : ControllerBase
{
    private readonly OracleDbContext _db;

    public OwnedBgController(OracleDbContext context)
    {
        _db = context;
    }

    /// <summary>
    /// 检查用户是否拥有指定背景
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="bgUrl">背景图片链接</param>
    /// <returns>1表示已拥有，0表示没有</returns>
    [HttpGet("check-if-the-bg-owned")]
    [SwaggerOperation(Summary = "检查用户是否拥有指定背景")]
    [SwaggerResponse(200, "检查成功", typeof(int))]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<int>> CheckIfBgOwned([FromQuery] int userId, [FromQuery] string bgUrl)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(bgUrl))
        {
            return BadRequest("用户ID和背景链接不能为空");
        }

        try
        {
            var exists = await _db.OwnedBgs.AnyAsync(ob => ob.UserId == userId && ob.BgUrl == bgUrl);
            return Ok(exists ? 1 : 0);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买背景
    /// </summary>
    /// <param name="request">购买背景请求</param>
    /// <returns>购买结果</returns>
    [HttpPost("buy-one-bg")]
    [SwaggerOperation(Summary = "购买背景")]
    [SwaggerResponse(200, "购买成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(409, "背景已拥有")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> BuyOneBg([FromBody] BuyBgRequest request)
    {
        if (request == null || request.UserId <= 0 || 
            string.IsNullOrWhiteSpace(request.BgUrl) || 
            string.IsNullOrWhiteSpace(request.BgName))
        {
            return BadRequest("请求参数不能为空");
        }

        try
        {
            // 检查是否已经拥有
            var exists = await _db.OwnedBgs.AnyAsync(ob => ob.UserId == request.UserId && ob.BgUrl == request.BgUrl);
            if (exists)
            {
                return Conflict("用户已拥有此背景");
            }

            // 简化主键生成
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var nextId = (int)(timestamp % int.MaxValue);
            
            while (await _db.OwnedBgs.AnyAsync(ob => ob.OwnedBgId == nextId))
            {
                nextId++;
            }

            var ownedBg = new OwnedBg
            {
                OwnedBgId = nextId,
                UserId = request.UserId,
                BgUrl = request.BgUrl,
                BgName = request.BgName,
                PurchaseDate = DateTime.Now
            };

            _db.OwnedBgs.Add(ownedBg);
            await _db.SaveChangesAsync();

            return Ok("购买成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户拥有的所有背景
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户拥有的背景列表</returns>
    [HttpGet("get-my-bg-list/{userId}")]
    [SwaggerOperation(Summary = "获取用户拥有的所有背景")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> GetMyBgList(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var ownedBgs = await _db.OwnedBgs
                .Where(ob => ob.UserId == userId)
                .Select(ob => new { url = ob.BgUrl, name = ob.BgName })
                .ToListAsync();

            return Ok(ownedBgs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}