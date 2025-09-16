/*
 * Project Name:  DatabaseWebAPI
 * File Name:     NotificationHub.cs
 * File Function: 实时通知Hub
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace DatabaseWebAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // 存储用户连接映射
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections 
        = new ConcurrentDictionary<string, HashSet<string>>();
    
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // 将连接添加到用户组
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // 记录用户连接
            UserConnections.AddOrUpdate(userId, 
                new HashSet<string> { Context.ConnectionId },
                (key, connections) => 
                {
                    connections.Add(Context.ConnectionId);
                    return connections;
                });

            _logger.LogInformation("用户 {UserId} 建立连接: {ConnectionId}", userId, Context.ConnectionId);
            
            // 通知用户上线状态
            await Clients.Group($"User_{userId}").SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // 从用户组移除连接
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // 移除用户连接记录
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                    // 通知用户离线状态
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            _logger.LogInformation("用户 {UserId} 断开连接: {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // 发送私信
    public async Task SendPrivateMessage(string receiverId, string message)
    {
        var senderId = Context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(senderId))
            return;

        var messageData = new
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message,
            Timestamp = DateTime.Now,
            MessageId = Guid.NewGuid().ToString()
        };

        // 发送给接收者
        await Clients.Group($"User_{receiverId}").SendAsync("ReceivePrivateMessage", messageData);
        
        // 发送给发送者（确认发送成功）
        await Clients.Caller.SendAsync("MessageSent", messageData);

        _logger.LogInformation("私信发送: {SenderId} -> {ReceiverId}", senderId, receiverId);
    }

    // 加入群聊
    public async Task JoinGroup(string groupId)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
            await Clients.Group($"Group_{groupId}").SendAsync("UserJoinedGroup", userId, groupId);
            
            _logger.LogInformation("用户 {UserId} 加入群聊 {GroupId}", userId, groupId);
        }
    }

    // 离开群聊
    public async Task LeaveGroup(string groupId)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
            await Clients.Group($"Group_{groupId}").SendAsync("UserLeftGroup", userId, groupId);
            
            _logger.LogInformation("用户 {UserId} 离开群聊 {GroupId}", userId, groupId);
        }
    }

    // 发送群消息
    public async Task SendGroupMessage(string groupId, string message)
    {
        var senderId = Context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(senderId))
            return;

        var messageData = new
        {
            SenderId = senderId,
            GroupId = groupId,
            Message = message,
            Timestamp = DateTime.Now,
            MessageId = Guid.NewGuid().ToString()
        };

        await Clients.Group($"Group_{groupId}").SendAsync("ReceiveGroupMessage", messageData);
        
        _logger.LogInformation("群消息发送: {SenderId} -> Group_{GroupId}", senderId, groupId);
    }

    // 发送系统通知给特定用户
    public async Task SendNotificationToUser(string userId, object notification)
    {
        await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
    }

    // 发送系统广播
    public async Task SendBroadcast(object announcement)
    {
        await Clients.All.SendAsync("ReceiveBroadcast", announcement);
    }

    // 获取在线用户数
    public static int GetOnlineUserCount()
    {
        return UserConnections.Count;
    }

    // 检查用户是否在线
    public static bool IsUserOnline(string userId)
    {
        return UserConnections.ContainsKey(userId);
    }

    // 获取用户的所有连接
    public static HashSet<string>? GetUserConnections(string userId)
    {
        UserConnections.TryGetValue(userId, out var connections);
        return connections;
    }
}

// 通知服务接口
public interface INotificationService
{
    Task SendNotificationToUserAsync(string userId, NotificationDto notification);
    Task SendNotificationToUsersAsync(List<string> userIds, NotificationDto notification);
    Task SendBroadcastAsync(NotificationDto notification);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task MarkNotificationAsReadAsync(string userId, string notificationId);
    Task MarkAllNotificationsAsReadAsync(string userId);
    Task<int> GetUnreadNotificationCountAsync(string userId);
}

// 通知服务实现
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;
    
    // 这里应该使用数据库或缓存存储通知，现在用内存模拟
    private static readonly ConcurrentDictionary<string, List<NotificationDto>> UserNotifications
        = new ConcurrentDictionary<string, List<NotificationDto>>();

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(string userId, NotificationDto notification)
    {
        notification.Id = Guid.NewGuid().ToString();
        notification.CreatedAt = DateTime.Now;
        notification.IsRead = false;

        // 存储通知
        UserNotifications.AddOrUpdate(userId,
            new List<NotificationDto> { notification },
            (key, notifications) =>
            {
                notifications.Insert(0, notification);
                // 只保留最近500条通知
                if (notifications.Count > 500)
                {
                    notifications.RemoveRange(500, notifications.Count - 500);
                }
                return notifications;
            });

        // 实时推送通知
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        
        _logger.LogInformation("发送通知给用户 {UserId}: {Title}", userId, notification.Title);
    }

    public async Task SendNotificationToUsersAsync(List<string> userIds, NotificationDto notification)
    {
        var tasks = userIds.Select(userId => SendNotificationToUserAsync(userId, notification));
        await Task.WhenAll(tasks);
    }

    public async Task SendBroadcastAsync(NotificationDto notification)
    {
        notification.Id = Guid.NewGuid().ToString();
        notification.CreatedAt = DateTime.Now;
        notification.IsRead = false;

        await _hubContext.Clients.All.SendAsync("ReceiveBroadcast", notification);
        
        _logger.LogInformation("发送系统广播: {Title}", notification.Title);
    }

    public Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        if (!UserNotifications.TryGetValue(userId, out var notifications))
        {
            return Task.FromResult(new List<NotificationDto>());
        }

        var skip = (page - 1) * pageSize;
        var result = notifications.Skip(skip).Take(pageSize).ToList();
        
        return Task.FromResult(result);
    }

    public Task MarkNotificationAsReadAsync(string userId, string notificationId)
    {
        if (UserNotifications.TryGetValue(userId, out var notifications))
        {
            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }
        }

        return Task.CompletedTask;
    }

    public Task MarkAllNotificationsAsReadAsync(string userId)
    {
        if (UserNotifications.TryGetValue(userId, out var notifications))
        {
            foreach (var notification in notifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }
        }

        return Task.CompletedTask;
    }

    public Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        if (!UserNotifications.TryGetValue(userId, out var notifications))
        {
            return Task.FromResult(0);
        }

        var count = notifications.Count(n => !n.IsRead);
        return Task.FromResult(count);
    }
}

// 通知数据传输对象
public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
    public string? SenderUserId { get; set; }
    public string? SenderUserName { get; set; }
    public string? SenderAvatarUrl { get; set; }
}

// 通知类型枚举
public enum NotificationType
{
    System = 0,        // 系统通知
    Like = 1,          // 点赞通知
    Comment = 2,       // 评论通知
    Follow = 3,        // 关注通知
    Mention = 4,       // @提及通知
    PrivateMessage = 5, // 私信通知
    GroupMessage = 6,   // 群消息通知
    LevelUp = 7,       // 升级通知
    Badge = 8,         // 勋章通知
    Announcement = 9   // 公告通知
}
