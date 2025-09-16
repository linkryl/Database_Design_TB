/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserTaskProgress.cs
 * File Function: 用户任务进度模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_TASK_PROGRESS")]
[SwaggerSchema(Description = "用户任务进度表")]
public sealed class UserTaskProgress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("PROGRESS_ID")]
    [SwaggerSchema("进度ID")]
    public int ProgressId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("TASK_ID")]
    [SwaggerSchema("任务ID")]
    public int TaskId { get; set; }

    [Column("CURRENT_PROGRESS")]
    [SwaggerSchema("当前进度")]
    public int CurrentProgress { get; set; } = 0;

    [Required]
    [Column("TARGET_PROGRESS")]
    [SwaggerSchema("目标进度")]
    public int TargetProgress { get; set; }

    [Column("IS_COMPLETED")]
    [SwaggerSchema("是否完成")]
    public int IsCompleted { get; set; } = 0;

    [Column("COMPLETED_TIME")]
    [SwaggerSchema("完成时间")]
    public DateTime? CompletedTime { get; set; }

    [Column("RESET_TIME")]
    [SwaggerSchema("重置时间（用于每日/每周任务）")]
    public DateTime? ResetTime { get; set; }

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("TaskId")]
    public UserTask? Task { get; set; }
}


