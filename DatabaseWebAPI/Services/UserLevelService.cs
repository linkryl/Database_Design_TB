/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserLevelService.cs
 * File Function: 用户等级系统服务
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.EntityFrameworkCore;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.TableModels;
using System.Text.Json;

namespace DatabaseWebAPI.Services;

public interface IUserLevelService
{
    Task<UserLevelInfo> GetUserLevelInfoAsync(int userId);
    Task<bool> AddExperienceAsync(int userId, int experience, string actionType, string description, int? relatedId = null);
    Task<CheckInResult> CheckInAsync(int userId);
    Task<List<UserBadgeDto>> GetUserBadgesAsync(int userId);
    Task<List<UserTaskDto>> GetUserTasksAsync(int userId);
    Task<bool> CompleteTaskAsync(int userId, int taskId);
    Task CheckAndAwardBadgesAsync(int userId, string actionType, object? actionData = null);
    Task<List<UserLevelConfig>> GetAllLevelsAsync();
    Task<UserRankInfo> GetUserRankAsync(int userId);
    Task<List<object>> GetLeaderboardAsync(int limit = 50);
    Task<List<object>> GetCheckInHistoryAsync(int userId, int days = 30);
    Task<List<object>> GetExperienceLogAsync(int userId, int page = 1, int pageSize = 20);
}

public class UserLevelService : IUserLevelService
{
    private readonly OracleDbContext _context;
    private readonly ILogger<UserLevelService> _logger;

    public UserLevelService(OracleDbContext context, ILogger<UserLevelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserLevelInfo> GetUserLevelInfoAsync(int userId)
    {
        var user = await _context.UserSet.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("用户不存在");

        var levelConfig = await GetUserCurrentLevel(user.ExperiencePoints);
        var nextLevelConfig = await GetNextLevel(levelConfig.LevelId);

        // 计算等级进度
        var progressInCurrentLevel = user.ExperiencePoints - levelConfig.MinExperience;
        var experienceNeededForCurrentLevel = levelConfig.MaxExperience - levelConfig.MinExperience;
        var progressPercentage = (double)progressInCurrentLevel / experienceNeededForCurrentLevel * 100;

        // 获取用户勋章数量
        var badgeCount = await _context.Set<UserBadgeRelation>()
            .CountAsync(ubr => ubr.UserId == userId);

        // 获取连续签到天数
        var consecutiveDays = await GetConsecutiveCheckInDays(userId);

        return new UserLevelInfo
        {
            UserId = userId,
            CurrentLevel = levelConfig.LevelId,
            LevelName = levelConfig.LevelName,
            LevelIcon = levelConfig.LevelIcon ?? "🌱",
            LevelColor = levelConfig.LevelColor,
            CurrentExperience = user.ExperiencePoints,
            ExperienceToNextLevel = nextLevelConfig?.MinExperience - user.ExperiencePoints ?? 0,
            ProgressPercentage = Math.Min(100, Math.Max(0, progressPercentage)),
            BadgeCount = badgeCount,
            ConsecutiveCheckInDays = consecutiveDays,
            Privileges = ParsePrivileges(levelConfig.Privileges ?? "{}"),
            DailyPostLimit = levelConfig.DailyPostLimit,
            DailyCommentLimit = levelConfig.DailyCommentLimit,
            CanCreateBar = levelConfig.CanCreateBar == 1,
            CanPinPost = levelConfig.CanPinPost == 1,
            StorageQuota = levelConfig.StorageQuota
        };
    }

    public async Task<bool> AddExperienceAsync(int userId, int experience, string actionType, string description, int? relatedId = null)
    {
        try
        {
            var user = await _context.UserSet.FindAsync(userId);
            if (user == null) return false;

            var oldExperience = user.ExperiencePoints;
            user.ExperiencePoints += experience;

            // 记录经验值变化日志
            _context.Set<UserExperienceLog>().Add(new UserExperienceLog
            {
                UserId = userId,
                ExperienceChange = experience,
                ActionType = actionType,
                ActionDescription = description,
                RelatedId = relatedId,
                CreatedTime = DateTime.Now
            });

            await _context.SaveChangesAsync();

            // 检查等级变化
            await CheckLevelUpAsync(userId, oldExperience, user.ExperiencePoints);

            // 检查并授予勋章
            await CheckAndAwardBadgesAsync(userId, actionType, new { experience, relatedId });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加用户经验值失败: UserId={UserId}, Experience={Experience}", userId, experience);
            return false;
        }
    }

    public async Task<CheckInResult> CheckInAsync(int userId)
    {
        try
        {
            var today = DateTime.Today;
            
            // 检查今日是否已签到
            var todayCheckIn = await _context.Set<UserCheckIn>()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CheckInDate == today);

            if (todayCheckIn != null)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "今日已签到",
                    ConsecutiveDays = todayCheckIn.ConsecutiveDays,
                    ExperienceGained = 0
                };
            }

            // 获取昨日签到记录
            var yesterday = today.AddDays(-1);
            var yesterdayCheckIn = await _context.Set<UserCheckIn>()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CheckInDate == yesterday);

            // 计算连续签到天数
            var consecutiveDays = yesterdayCheckIn?.ConsecutiveDays + 1 ?? 1;

            // 计算奖励经验值（连续签到有额外奖励）
            var baseExperience = 5;
            var bonusExperience = 0;
            var bonusApplied = false;

            if (consecutiveDays >= 7)
            {
                bonusExperience = Math.Min(consecutiveDays / 7 * 5, 20); // 每7天额外5经验，最多20
                bonusApplied = true;
            }

            var totalExperience = baseExperience + bonusExperience;

            // 创建签到记录
            var checkIn = new UserCheckIn
            {
                UserId = userId,
                CheckInDate = today,
                ConsecutiveDays = consecutiveDays,
                ExperienceGained = totalExperience,
                BonusApplied = bonusApplied ? 1 : 0,
                CreatedTime = DateTime.Now
            };

            _context.Set<UserCheckIn>().Add(checkIn);

            // 添加经验值
            await AddExperienceAsync(userId, totalExperience, "CHECK_IN", $"每日签到，连续{consecutiveDays}天");

            // 更新任务进度
            await UpdateTaskProgressAsync(userId, "daily_checkin", 1);

            await _context.SaveChangesAsync();

            return new CheckInResult
            {
                Success = true,
                Message = bonusApplied ? $"签到成功！连续签到{consecutiveDays}天，获得额外奖励！" : "签到成功！",
                ConsecutiveDays = consecutiveDays,
                ExperienceGained = totalExperience,
                BonusApplied = bonusApplied
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户签到失败: UserId={UserId}", userId);
            return new CheckInResult
            {
                Success = false,
                Message = "签到失败，请稍后重试",
                ConsecutiveDays = 0,
                ExperienceGained = 0
            };
        }
    }

    public async Task<List<UserBadgeDto>> GetUserBadgesAsync(int userId)
    {
        var badges = await _context.Set<UserBadgeRelation>()
            .Include(ubr => ubr.Badge)
            .Where(ubr => ubr.UserId == userId)
            .OrderByDescending(ubr => ubr.EarnedDate)
            .Select(ubr => new UserBadgeDto
            {
                BadgeId = ubr.Badge!.BadgeId,
                BadgeName = ubr.Badge.BadgeName,
                BadgeDesc = ubr.Badge.BadgeDesc ?? "",
                BadgeIcon = ubr.Badge.BadgeIcon ?? "",
                BadgeType = ubr.Badge.BadgeType,
                BadgeRarity = ubr.Badge.BadgeRarity,
                EarnedDate = ubr.EarnedDate,
                IsDisplayed = ubr.IsDisplayed == 1
            })
            .ToListAsync();

        return badges;
    }

    public async Task<List<UserTaskDto>> GetUserTasksAsync(int userId)
    {
        var today = DateTime.Today;
        var tasks = await _context.Set<UserTask>()
            .Where(t => t.IsActive == 1 && (t.EndTime == null || t.EndTime >= today))
            .ToListAsync();

        var userTasks = new List<UserTaskDto>();

        foreach (var task in tasks)
        {
            var progress = await GetTaskProgress(userId, task.TaskId);
            var taskCondition = JsonSerializer.Deserialize<Dictionary<string, object>>(task.TaskCondition);
            var targetProgress = taskCondition.ContainsKey("count") ? 
                Convert.ToInt32(taskCondition["count"]) : 1;

            userTasks.Add(new UserTaskDto
            {
                TaskId = task.TaskId,
                TaskName = task.TaskName,
                TaskDesc = task.TaskDesc ?? "",
                TaskType = task.TaskType,
                ExperienceReward = task.ExperienceReward,
                CurrentProgress = progress.CurrentProgress,
                TargetProgress = targetProgress,
                IsCompleted = progress.IsCompleted == 1,
                CompletedTime = progress.CompletedTime
            });
        }

        return userTasks;
    }

    public async Task<bool> CompleteTaskAsync(int userId, int taskId)
    {
        try
        {
            var task = await _context.Set<Models.TableModels.UserTask>().FindAsync(taskId);
            if (task == null) return false;

            var progress = await GetTaskProgress(userId, taskId);
            if (progress.IsCompleted == 1) return false;

            // 标记任务完成
            progress.IsCompleted = 1;
            progress.CompletedTime = DateTime.Now;
            _context.Set<UserTaskProgress>().Update(progress);

            // 给予奖励
            if (task.ExperienceReward > 0)
            {
                await AddExperienceAsync(userId, task.ExperienceReward, "TASK_COMPLETE", $"完成任务：{task.TaskName}");
            }

            if (task.BadgeReward.HasValue)
            {
                await AwardBadgeAsync(userId, task.BadgeReward.Value);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "完成任务失败: UserId={UserId}, TaskId={TaskId}", userId, taskId);
            return false;
        }
    }

    public async Task CheckAndAwardBadgesAsync(int userId, string actionType, object? actionData = null)
    {
        try
        {
            var badges = await _context.Set<Models.TableModels.UserBadge>()
                .Where(b => b.IsActive == 1)
                .ToListAsync();

            foreach (var badge in badges)
            {
                if (await ShouldAwardBadge(userId, badge, actionType, actionData))
                {
                    await AwardBadgeAsync(userId, badge.BadgeId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查并授予勋章失败: UserId={UserId}", userId);
        }
    }

    public async Task<List<UserLevelConfig>> GetAllLevelsAsync()
    {
        return await _context.Set<UserLevelConfig>()
            .OrderBy(l => l.LevelId)
            .ToListAsync();
    }

    public async Task<UserRankInfo> GetUserRankAsync(int userId)
    {
        var user = await _context.UserSet.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("用户不存在");

        // 计算经验值排名
        var experienceRank = await _context.UserSet
            .CountAsync(u => u.ExperiencePoints > user.ExperiencePoints) + 1;

        // 计算总用户数
        var totalUsers = await _context.UserSet.CountAsync();

        // 计算百分比排名
        var percentile = (double)(totalUsers - experienceRank + 1) / totalUsers * 100;

        return new UserRankInfo
        {
            UserId = userId,
            ExperienceRank = experienceRank,
            TotalUsers = totalUsers,
            Percentile = Math.Round(percentile, 1)
        };
    }

    // 添加缺失的方法
    public async Task<List<object>> GetLeaderboardAsync(int limit = 50)
    {
        var leaderboard = await _context.UserSet
            .OrderByDescending(u => u.ExperiencePoints)
            .Take(limit)
            .Select((u, index) => new
            {
                rank = index + 1,
                userId = u.UserId,
                userName = u.UserName,
                avatarUrl = u.AvatarUrl,
                experiencePoints = u.ExperiencePoints,
                level = CalculateUserLevelFromExperience(u.ExperiencePoints)
            })
            .ToListAsync();

        return leaderboard.Cast<object>().ToList();
    }

    public async Task<List<object>> GetCheckInHistoryAsync(int userId, int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);
        
        var history = await _context.Set<UserCheckIn>()
            .Where(c => c.UserId == userId && c.CheckInDate >= startDate)
            .OrderByDescending(c => c.CheckInDate)
            .Select(c => new
            {
                checkInDate = c.CheckInDate,
                consecutiveDays = c.ConsecutiveDays,
                experienceGained = c.ExperienceGained,
                bonusApplied = c.BonusApplied == 1
            })
            .ToListAsync();

        return history.Cast<object>().ToList();
    }

    public async Task<List<object>> GetExperienceLogAsync(int userId, int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;
        
        var log = await _context.Set<UserExperienceLog>()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedTime)
            .Skip(skip)
            .Take(pageSize)
            .Select(l => new
            {
                logId = l.LogId,
                experienceChange = l.ExperienceChange,
                actionType = l.ActionType,
                actionDescription = l.ActionDescription,
                relatedId = l.RelatedId,
                createdTime = l.CreatedTime
            })
            .ToListAsync();

        return log.Cast<object>().ToList();
    }

    private int CalculateUserLevelFromExperience(int experience)
    {
        // 简单的等级计算逻辑
        if (experience < 100) return 1;
        if (experience < 300) return 2;
        if (experience < 700) return 3;
        if (experience < 1500) return 4;
        if (experience < 3000) return 5;
        if (experience < 6000) return 6;
        if (experience < 10000) return 7;
        return 8; // 最高等级
    }

    #region 私有方法

    private async Task<UserLevelConfig> GetUserCurrentLevel(int experience)
    {
        return await _context.Set<UserLevelConfig>()
            .FirstAsync(l => experience >= l.MinExperience && experience <= l.MaxExperience);
    }

    private async Task<UserLevelConfig?> GetNextLevel(int currentLevelId)
    {
        return await _context.Set<UserLevelConfig>()
            .FirstOrDefaultAsync(l => l.LevelId == currentLevelId + 1);
    }

    private async Task<int> GetConsecutiveCheckInDays(int userId)
    {
        var latestCheckIn = await _context.Set<UserCheckIn>()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CheckInDate)
            .FirstOrDefaultAsync();

        if (latestCheckIn == null) return 0;

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        // 如果最近签到是今天或昨天，返回连续天数
        if (latestCheckIn.CheckInDate == today || latestCheckIn.CheckInDate == yesterday)
        {
            return latestCheckIn.ConsecutiveDays;
        }

        return 0; // 签到中断
    }

    private async Task CheckLevelUpAsync(int userId, int oldExperience, int newExperience)
    {
        var oldLevel = await GetUserCurrentLevel(oldExperience);
        var newLevel = await GetUserCurrentLevel(newExperience);

        if (newLevel.LevelId > oldLevel.LevelId)
        {
            // 记录等级变更
            _context.Set<UserLevelLog>().Add(new UserLevelLog
            {
                UserId = userId,
                OldLevel = oldLevel.LevelId,
                NewLevel = newLevel.LevelId,
                ExperienceAtChange = newExperience,
                UpgradeTime = DateTime.Now
            });

            // 检查等级勋章
            await CheckLevelBadges(userId, newLevel.LevelId);
        }
    }

    private async Task CheckLevelBadges(int userId, int level)
    {
        var levelBadges = await _context.Set<Models.TableModels.UserBadge>()
            .Where(b => b.BadgeType == 4 && b.IsActive == 1) // 等级勋章
            .ToListAsync();

        foreach (var badge in levelBadges)
        {
            var condition = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.UnlockCondition);
            if (condition.ContainsKey("user_level") && Convert.ToInt32(condition["user_level"]) <= level)
            {
                await AwardBadgeAsync(userId, badge.BadgeId);
            }
        }
    }

    private async Task<bool> ShouldAwardBadge(int userId, Models.TableModels.UserBadge badge, string actionType, object? actionData)
    {
        // 检查用户是否已有此勋章
        var hasBadge = await _context.Set<UserBadgeRelation>()
            .AnyAsync(ubr => ubr.UserId == userId && ubr.BadgeId == badge.BadgeId);

        if (hasBadge) return false;

        var condition = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.UnlockCondition ?? "{}");
        var conditionType = condition?["type"]?.ToString();

        return conditionType switch
        {
            "first_login" => actionType == "LOGIN",
            "first_post" => actionType == "POST" && await _context.PostSet.CountAsync(p => p.UserId == userId) == 1,
            "consecutive_checkin" => await CheckConsecutiveCheckIn(userId, Convert.ToInt32(condition?["days"] ?? 0)),
            "post_likes" => await CheckPostLikes(userId, Convert.ToInt32(condition?["count"] ?? 0)),
            "comment_count" => await CheckCommentCount(userId, Convert.ToInt32(condition?["count"] ?? 0)),
            "favorite_count" => await CheckFavoriteCount(userId, Convert.ToInt32(condition?["count"] ?? 0)),
            "following_count" => await CheckFollowingCount(userId, Convert.ToInt32(condition?["count"] ?? 0)),
            "create_bar" => actionType == "CREATE_BAR",
            "registration_days" => await CheckRegistrationDays(userId, Convert.ToInt32(condition?["days"] ?? 0)),
            _ => false
        };
    }

    private async Task<bool> AwardBadgeAsync(int userId, int badgeId)
    {
        try
        {
            var existing = await _context.Set<UserBadgeRelation>()
                .FirstOrDefaultAsync(ubr => ubr.UserId == userId && ubr.BadgeId == badgeId);

            if (existing != null) return false;

            _context.Set<UserBadgeRelation>().Add(new UserBadgeRelation
            {
                UserId = userId,
                BadgeId = badgeId,
                EarnedDate = DateTime.Now,
                IsDisplayed = 1
            });

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "授予勋章失败: UserId={UserId}, BadgeId={BadgeId}", userId, badgeId);
            return false;
        }
    }

    private async Task<UserTaskProgress> GetTaskProgress(int userId, int taskId)
    {
        var today = DateTime.Today;
        var task = await _context.Set<Models.TableModels.UserTask>().FindAsync(taskId);
        if (task == null) throw new ArgumentException("任务不存在");
        
        DateTime? resetTime = task.TaskType switch
        {
            1 => today, // 每日任务
            2 => today.AddDays(-(int)today.DayOfWeek), // 每周任务，重置到周日
            _ => null // 其他任务不重置
        };

        var progress = await _context.Set<UserTaskProgress>()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TaskId == taskId && 
                                    (resetTime == null || p.ResetTime == resetTime));

        if (progress == null)
        {
            var taskCondition = JsonSerializer.Deserialize<Dictionary<string, object>>(task.TaskCondition) ?? new Dictionary<string, object>();
            var targetProgress = taskCondition.ContainsKey("count") ? 
                Convert.ToInt32(taskCondition["count"]) : 1;

            progress = new UserTaskProgress
            {
                UserId = userId,
                TaskId = taskId,
                CurrentProgress = 0,
                TargetProgress = targetProgress,
                IsCompleted = 0,
                ResetTime = resetTime,
                CreatedTime = DateTime.Now
            };

            _context.Set<UserTaskProgress>().Add(progress);
            await _context.SaveChangesAsync();
        }

        return progress;
    }

    private async Task UpdateTaskProgressAsync(int userId, string actionType, int increment)
    {
        var tasks = await _context.Set<Models.TableModels.UserTask>()
            .Where(t => t.IsActive == 1)
            .ToListAsync();

        foreach (var task in tasks)
        {
            var condition = JsonSerializer.Deserialize<Dictionary<string, object>>(task.TaskCondition) ?? new Dictionary<string, object>();
            if (condition.ContainsKey("type") && condition["type"]?.ToString() == actionType)
            {
                var progress = await GetTaskProgress(userId, task.TaskId);
                progress.CurrentProgress += increment;

                if (progress.CurrentProgress >= progress.TargetProgress && progress.IsCompleted == 0)
                {
                    await CompleteTaskAsync(userId, task.TaskId);
                }
                else
                {
                    _context.Set<UserTaskProgress>().Update(progress);
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private Dictionary<string, object> ParsePrivileges(string privilegesJson)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(privilegesJson) 
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    // 检查方法实现
    private async Task<bool> CheckConsecutiveCheckIn(int userId, int days)
    {
        return await GetConsecutiveCheckInDays(userId) >= days;
    }

    private async Task<bool> CheckPostLikes(int userId, int count)
    {
        return await _context.PostSet
            .Where(p => p.UserId == userId)
            .AnyAsync(p => p.LikeCount >= count);
    }

    private async Task<bool> CheckCommentCount(int userId, int count)
    {
        return await _context.PostCommentSet.CountAsync(c => c.UserId == userId) >= count;
    }

    private async Task<bool> CheckFavoriteCount(int userId, int count)
    {
        return await _context.PostFavoriteSet.CountAsync(f => f.UserId == userId) >= count;
    }

    private async Task<bool> CheckFollowingCount(int userId, int count)
    {
        return await _context.UserFollowSet.CountAsync(f => f.FollowerId == userId) >= count;
    }

    private async Task<bool> CheckRegistrationDays(int userId, int days)
    {
        var user = await _context.UserSet.FindAsync(userId);
        if (user == null) return false;

        var daysSinceRegistration = (DateTime.Now - user.RegistrationDate).Days;
        return daysSinceRegistration >= days;
    }

    #endregion
}

// 数据传输对象（DTO）
public class UserLevelInfo
{
    public int UserId { get; set; }
    public int CurrentLevel { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string LevelIcon { get; set; } = string.Empty;
    public string LevelColor { get; set; } = string.Empty;
    public int CurrentExperience { get; set; }
    public int ExperienceToNextLevel { get; set; }
    public double ProgressPercentage { get; set; }
    public int BadgeCount { get; set; }
    public int ConsecutiveCheckInDays { get; set; }
    public Dictionary<string, object> Privileges { get; set; } = new();
    public int DailyPostLimit { get; set; }
    public int DailyCommentLimit { get; set; }
    public bool CanCreateBar { get; set; }
    public bool CanPinPost { get; set; }
    public long StorageQuota { get; set; }
}

public class CheckInResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ConsecutiveDays { get; set; }
    public int ExperienceGained { get; set; }
    public bool BonusApplied { get; set; }
}

public class UserBadgeDto
{
    public int BadgeId { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string BadgeDesc { get; set; } = string.Empty;
    public string BadgeIcon { get; set; } = string.Empty;
    public int BadgeType { get; set; }
    public int BadgeRarity { get; set; }
    public DateTime EarnedDate { get; set; }
    public bool IsDisplayed { get; set; }
}

public class UserTaskDto
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string TaskDesc { get; set; } = string.Empty;
    public int TaskType { get; set; }
    public int ExperienceReward { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedTime { get; set; }
}

public class UserRankInfo
{
    public int UserId { get; set; }
    public int ExperienceRank { get; set; }
    public int TotalUsers { get; set; }
    public double Percentile { get; set; }
}
