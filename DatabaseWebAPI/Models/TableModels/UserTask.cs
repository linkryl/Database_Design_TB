/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserTask.cs
 * File Function: 用户任务模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_TASK")]
[SwaggerSchema(Description = "用户任务配置表")]
public sealed class UserTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("TASK_ID")]
    [SwaggerSchema("任务ID")]
    public int TaskId { get; set; }

    [Required]
    [Column("TASK_NAME")]
    [StringLength(64)]
    [SwaggerSchema("任务名称")]
    public string TaskName { get; set; } = string.Empty;

    [Column("TASK_DESC")]
    [StringLength(256)]
    [SwaggerSchema("任务描述")]
    public string? TaskDesc { get; set; }

    [Required]
    [Column("TASK_TYPE")]
    [SwaggerSchema("任务类型：1每日任务 2每周任务 3成长任务 4限时任务")]
    public int TaskType { get; set; }

    [Required]
    [Column("TASK_CONDITION")]
    [SwaggerSchema("任务条件JSON")]
    public string TaskCondition { get; set; } = string.Empty;

    [Column("EXPERIENCE_REWARD")]
    [SwaggerSchema("经验值奖励")]
    public int ExperienceReward { get; set; } = 0;

    [Column("BADGE_REWARD")]
    [SwaggerSchema("勋章奖励ID")]
    public int? BadgeReward { get; set; }

    [Column("OTHER_REWARDS")]
    [SwaggerSchema("其他奖励JSON")]
    public string? OtherRewards { get; set; }

    [Column("IS_ACTIVE")]
    [SwaggerSchema("是否激活")]
    public int IsActive { get; set; } = 1;

    [Column("START_TIME")]
    [SwaggerSchema("开始时间")]
    public DateTime? StartTime { get; set; }

    [Column("END_TIME")]
    [SwaggerSchema("结束时间")]
    public DateTime? EndTime { get; set; }

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("BadgeReward")]
    public UserBadge? BadgeRewardNavigation { get; set; }

    public ICollection<UserTaskProgress> UserTaskProgresses { get; set; } = new HashSet<UserTaskProgress>();
}


