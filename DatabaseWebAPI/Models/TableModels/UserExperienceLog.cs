/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserExperienceLog.cs
 * File Function: 用户经验值记录模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_EXPERIENCE_LOG")]
[SwaggerSchema(Description = "用户经验值获取记录表")]
public sealed class UserExperienceLog
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
    [Column("EXPERIENCE_CHANGE")]
    [SwaggerSchema("经验值变化（正数为获得，负数为扣除）")]
    public int ExperienceChange { get; set; }

    [Required]
    [Column("ACTION_TYPE")]
    [StringLength(32)]
    [SwaggerSchema("行为类型：POST, COMMENT, LIKE_RECEIVED, CHECK_IN等")]
    public string ActionType { get; set; } = string.Empty;

    [Column("ACTION_DESCRIPTION")]
    [StringLength(256)]
    [SwaggerSchema("行为描述")]
    public string? ActionDescription { get; set; }

    [Column("RELATED_ID")]
    [SwaggerSchema("关联的帖子/评论等ID")]
    public int? RelatedId { get; set; }

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("UserId")]
    public User? User { get; set; }
}


