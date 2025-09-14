/*
 * Project Name:  DatabaseWebAPI
 * File Name:     BarFollow.cs
 * File Function: BarFollow 模型类 - 贴吧关注表
 * Author:        TreeHole开发组
 * Update Date:   2025-09-14
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("BAR_FOLLOW")]
[SwaggerSchema(Description = "贴吧关注表")]
[PrimaryKey(nameof(BarId), nameof(UserId))]
public sealed class BarFollow
{
    // 属性定义
    [Column("BAR_ID")]
    [SwaggerSchema("贴吧ID")]
    public int BarId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("FOLLOW_TIME")]
    [SwaggerSchema("关注时间")]
    public DateTime FollowTime { get; set; }

    [Required]
    [Column("IS_ACTIVE")]
    [SwaggerSchema("是否活跃关注：1=是，0=否")]
    public int IsActive { get; set; } = 1;

    [Column("NOTIFICATION_ENABLED")]
    [SwaggerSchema("是否开启通知：1=是，0=否")]
    public int NotificationEnabled { get; set; } = 1;

    // 暂时不添加导航属性，避免影响现有功能
}
