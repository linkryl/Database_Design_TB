/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserLevelConfig.cs
 * File Function: 用户等级配置模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_LEVEL_CONFIG")]
[SwaggerSchema(Description = "用户等级配置表")]
public sealed class UserLevelConfig
{
    [Key]
    [Column("LEVEL_ID")]
    [SwaggerSchema("等级ID")]
    public int LevelId { get; set; }

    [Required]
    [Column("LEVEL_NAME")]
    [StringLength(32)]
    [SwaggerSchema("等级名称")]
    public string LevelName { get; set; } = string.Empty;

    [Required]
    [Column("MIN_EXPERIENCE")]
    [SwaggerSchema("最小经验值")]
    public int MinExperience { get; set; }

    [Required]
    [Column("MAX_EXPERIENCE")]
    [SwaggerSchema("最大经验值")]
    public int MaxExperience { get; set; }

    [Column("LEVEL_ICON")]
    [StringLength(256)]
    [SwaggerSchema("等级图标")]
    public string? LevelIcon { get; set; }

    [Column("LEVEL_COLOR")]
    [StringLength(16)]
    [SwaggerSchema("等级颜色")]
    public string LevelColor { get; set; } = "#409eff";

    [Column("PRIVILEGES")]
    [SwaggerSchema("特权配置JSON")]
    public string? Privileges { get; set; }

    [Column("DAILY_POST_LIMIT")]
    [SwaggerSchema("每日发帖限制")]
    public int DailyPostLimit { get; set; } = 10;

    [Column("DAILY_COMMENT_LIMIT")]
    [SwaggerSchema("每日评论限制")]
    public int DailyCommentLimit { get; set; } = 50;

    [Column("CAN_CREATE_BAR")]
    [SwaggerSchema("是否可创建贴吧")]
    public int CanCreateBar { get; set; } = 0;

    [Column("CAN_PIN_POST")]
    [SwaggerSchema("是否可置顶帖子")]
    public int CanPinPost { get; set; } = 0;

    [Column("STORAGE_QUOTA")]
    [SwaggerSchema("存储配额(字节)")]
    public long StorageQuota { get; set; } = 104857600; // 100MB

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;
}

