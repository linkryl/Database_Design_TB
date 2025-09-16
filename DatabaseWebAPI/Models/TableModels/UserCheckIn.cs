/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserCheckIn.cs
 * File Function: 用户签到模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_CHECK_IN")]
[SwaggerSchema(Description = "用户签到记录表")]
public sealed class UserCheckIn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CHECK_IN_ID")]
    [SwaggerSchema("签到记录ID")]
    public int CheckInId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Column("CHECK_IN_DATE")]
    [SwaggerSchema("签到日期")]
    public DateTime CheckInDate { get; set; } = DateTime.Today;

    [Column("CONSECUTIVE_DAYS")]
    [SwaggerSchema("连续签到天数")]
    public int ConsecutiveDays { get; set; } = 1;

    [Column("EXPERIENCE_GAINED")]
    [SwaggerSchema("获得的经验值")]
    public int ExperienceGained { get; set; } = 5;

    [Column("BONUS_APPLIED")]
    [SwaggerSchema("是否应用了连续签到奖励")]
    public int BonusApplied { get; set; } = 0;

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("UserId")]
    public User? User { get; set; }
}


