/*
 * Project Name:  DatabaseWebAPI
 * File Name:     NotificationController.cs
 * File Function: 通知系统控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatabaseWebAPI.Hubs;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DatabaseWebAPI.Controllers.NotificationControllers;

[Route("api/notifications")]
[ApiController]
[SwaggerTag("实时通知系统 API")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    // 获取用户通知列表
    [HttpGet]
    [SwaggerOperation(Summary = "获取用户通知列表", Description = "获取当前用户的通知列表")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized("无效的用户身份");
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(
                userIdClaim.Value, page, pageSize);

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知列表失败");
            return StatusCode(500, "获取通知列表失败");
        }
    }

    // 获取未读通知数量
    [HttpGet("unread-count")]
    [SwaggerOperation(Summary = "获取未读通知数量", Description = "获取当前用户的未读通知数量")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(401, "未登录")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized("无效的用户身份");
            }

            var count = await _notificationService.GetUnreadNotificationCountAsync(userIdClaim.Value);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读通知数量失败");
            return StatusCode(500, "获取未读通知数量失败");
        }
    }

    // 标记通知为已读
    [HttpPut("{notificationId}/read")]
    [SwaggerOperation(Summary = "标记通知为已读", Description = "将指定通知标记为已读状态")]
    [SwaggerResponse(200, "标记成功")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(404, "通知不存在")]
    public async Task<ActionResult> MarkAsRead(string notificationId)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized("无效的用户身份");
            }

            await _notificationService.MarkNotificationAsReadAsync(userIdClaim.Value, notificationId);
            return Ok(new { message = "通知已标记为已读" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知已读失败: NotificationId={NotificationId}", notificationId);
            return StatusCode(500, "标记通知已读失败");
        }
    }

    // 标记所有通知为已读
    [HttpPut("read-all")]
    [SwaggerOperation(Summary = "标记所有通知为已读", Description = "将当前用户的所有通知标记为已读")]
    [SwaggerResponse(200, "标记成功")]
    [SwaggerResponse(401, "未登录")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized("无效的用户身份");
            }

            await _notificationService.MarkAllNotificationsAsReadAsync(userIdClaim.Value);
            return Ok(new { message = "所有通知已标记为已读" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记所有通知已读失败");
            return StatusCode(500, "标记所有通知已读失败");
        }
    }

    // 发送通知给指定用户（管理员功能）
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "发送通知", Description = "管理员发送通知给指定用户")]
    [SwaggerResponse(200, "发送成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(401, "未登录")]
    [SwaggerResponse(403, "权限不足")]
    public async Task<ActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest("标题和内容不能为空");
            }

            var notification = new NotificationDto
            {
                Title = request.Title,
                Content = request.Content,
                Type = request.Type,
                ActionUrl = request.ActionUrl,
                ActionData = request.ActionData
            };

            if (request.UserIds?.Any() == true)
            {
                // 发送给指定用户
                await _notificationService.SendNotificationToUsersAsync(request.UserIds, notification);
            }
            else
            {
                // 发送系统广播
                await _notificationService.SendBroadcastAsync(notification);
            }

            return Ok(new { message = "通知发送成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送通知失败");
            return StatusCode(500, "发送通知失败");
        }
    }

    // 获取在线用户数量
    [HttpGet("online-users")]
    [SwaggerOperation(Summary = "获取在线用户数量", Description = "获取当前在线用户数量")]
    [SwaggerResponse(200, "获取成功")]
    public ActionResult<object> GetOnlineUsers()
    {
        try
        {
            var onlineCount = NotificationHub.GetOnlineUserCount();
            return Ok(new { onlineCount, timestamp = DateTime.Now });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取在线用户数量失败");
            return StatusCode(500, "获取在线用户数量失败");
        }
    }

    // 检查用户是否在线
    [HttpGet("user/{userId}/online")]
    [SwaggerOperation(Summary = "检查用户是否在线", Description = "检查指定用户是否在线")]
    [SwaggerResponse(200, "检查成功")]
    public ActionResult<object> CheckUserOnline(string userId)
    {
        try
        {
            var isOnline = NotificationHub.IsUserOnline(userId);
            return Ok(new { userId, isOnline, timestamp = DateTime.Now });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户在线状态失败: UserId={UserId}", userId);
            return StatusCode(500, "检查用户在线状态失败");
        }
    }

    // 创建系统通知的辅助方法
    [HttpPost("system")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "创建系统通知", Description = "创建系统级通知")]
    public async Task<ActionResult> CreateSystemNotification([FromBody] SystemNotificationRequest request)
    {
        try
        {
            var notification = new NotificationDto
            {
                Title = request.Title,
                Content = request.Content,
                Type = NotificationType.System,
                ActionUrl = request.ActionUrl
            };

            switch (request.Target.ToLower())
            {
                case "all":
                    await _notificationService.SendBroadcastAsync(notification);
                    break;
                case "users":
                    if (request.UserIds?.Any() == true)
                    {
                        await _notificationService.SendNotificationToUsersAsync(request.UserIds, notification);
                    }
                    break;
                default:
                    return BadRequest("无效的目标类型");
            }

            return Ok(new { message = "系统通知创建成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建系统通知失败");
            return StatusCode(500, "创建系统通知失败");
        }
    }
}

// 请求模型
public class SendNotificationRequest
{
    [SwaggerSchema("通知标题")]
    public string Title { get; set; } = string.Empty;
    
    [SwaggerSchema("通知内容")]
    public string Content { get; set; } = string.Empty;
    
    [SwaggerSchema("通知类型")]
    public NotificationType Type { get; set; } = NotificationType.System;
    
    [SwaggerSchema("操作链接")]
    public string? ActionUrl { get; set; }
    
    [SwaggerSchema("操作数据")]
    public string? ActionData { get; set; }
    
    [SwaggerSchema("目标用户ID列表（为空则广播）")]
    public List<string>? UserIds { get; set; }
}

public class SystemNotificationRequest
{
    [SwaggerSchema("通知标题")]
    public string Title { get; set; } = string.Empty;
    
    [SwaggerSchema("通知内容")]
    public string Content { get; set; } = string.Empty;
    
    [SwaggerSchema("目标类型：all（所有用户）、users（指定用户）")]
    public string Target { get; set; } = "all";
    
    [SwaggerSchema("目标用户ID列表")]
    public List<string>? UserIds { get; set; }
    
    [SwaggerSchema("操作链接")]
    public string? ActionUrl { get; set; }
}
