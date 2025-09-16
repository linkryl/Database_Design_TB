/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserLevelLog.cs
 * File Function: 用户等级变更记录模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_LEVEL_LOG")]
[SwaggerSchema(Description = "用户等级变更记录表")]
public sealed class UserLevelLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("LOG_ID")]
    [SwaggerSchema("记录ID")]
    public int LogId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("OLD_LEVEL")]
    [SwaggerSchema("旧等级")]
    public int OldLevel { get; set; }

    [Required]
    [Column("NEW_LEVEL")]
    [SwaggerSchema("新等级")]
    public int NewLevel { get; set; }

    [Required]
    [Column("EXPERIENCE_AT_CHANGE")]
    [SwaggerSchema("升级时的经验值")]
    public int ExperienceAtChange { get; set; }

    [Column("UPGRADE_TIME")]
    [SwaggerSchema("升级时间")]
    public DateTime UpgradeTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("UserId")]
    public User? User { get; set; }
}


