/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserMessageController.cs
 * File Function: 用户私聊消息相关 API
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/user-message")]
[ApiController]
[SwaggerTag("用户私聊消息相关 API")]
public class UserMessageController(OracleDbContext context) : ControllerBase
{
    // 获取与某用户的所有私聊消息（双向）
    [HttpGet("{userId:int}/{otherUserId:int}")]
    [SwaggerOperation(Summary = "获取与某用户的所有私聊消息", Description = "获取与某用户的所有私聊消息（双向）")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<UserMessage>>> GetUserMessages(int userId, int otherUserId)
    {
        try
        {
            var messages = await context.UserMessageSet
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SendTime)
                .ToListAsync();
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 发送私聊消息
    [HttpPost]
    [SwaggerOperation(Summary = "发送私聊消息", Description = "发送私聊消息")]
    [SwaggerResponse(201, "发送成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> SendUserMessage([FromBody] UserMessage message)
    {
        if (!ModelState.IsValid)
        {
            // 输出所有ModelState错误到控制台
            Console.WriteLine(string.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }
        context.UserMessageSet.Add(message);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(SendUserMessage), new { id = message.MessageId }, message);
    }

    // 获取某用户收到的所有消息
    [HttpGet("received/{userId:int}")]
    [SwaggerOperation(Summary = "获取某用户收到的所有消息", Description = "获取某用户收到的所有消息")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<UserMessage>>> GetReceivedMessages(int userId)
    {
        try
        {
            var messages = await context.UserMessageSet
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SendTime)
                .ToListAsync();
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取某用户发送的所有消息
    [HttpGet("sent/{userId:int}")]
    [SwaggerOperation(Summary = "获取某用户发送的所有消息", Description = "获取某用户发送的所有消息")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<UserMessage>>> GetSentMessages(int userId)
    {
        try
        {
            var messages = await context.UserMessageSet
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SendTime)
                .ToListAsync();
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
