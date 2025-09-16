/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserBadgeRelation.cs
 * File Function: 用户勋章关联模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_BADGE_RELATION")]
[SwaggerSchema(Description = "用户勋章关联表")]
public sealed class UserBadgeRelation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("RELATION_ID")]
    [SwaggerSchema("关联ID")]
    public int RelationId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("BADGE_ID")]
    [SwaggerSchema("勋章ID")]
    public int BadgeId { get; set; }

    [Column("EARNED_DATE")]
    [SwaggerSchema("获得日期")]
    public DateTime EarnedDate { get; set; } = DateTime.Now;

    [Column("IS_DISPLAYED")]
    [SwaggerSchema("是否在个人资料中显示")]
    public int IsDisplayed { get; set; } = 1;

    // 导航属性
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("BadgeId")]
    public UserBadge? Badge { get; set; }
}


