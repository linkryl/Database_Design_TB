/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserBadge.cs
 * File Function: 用户勋章模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_BADGE")]
[SwaggerSchema(Description = "用户勋章表")]
public sealed class UserBadge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("BADGE_ID")]
    [SwaggerSchema("勋章ID")]
    public int BadgeId { get; set; }

    [Required]
    [Column("BADGE_NAME")]
    [StringLength(64)]
    [SwaggerSchema("勋章名称")]
    public string BadgeName { get; set; } = string.Empty;

    [Column("BADGE_DESC")]
    [StringLength(256)]
    [SwaggerSchema("勋章描述")]
    public string? BadgeDesc { get; set; }

    [Column("BADGE_ICON")]
    [StringLength(256)]
    [SwaggerSchema("勋章图标")]
    public string? BadgeIcon { get; set; }

    [Required]
    [Column("BADGE_TYPE")]
    [SwaggerSchema("勋章类型：1活跃度 2贡献度 3特殊成就 4等级勋章 5节日勋章")]
    public int BadgeType { get; set; }

    [Column("BADGE_RARITY")]
    [SwaggerSchema("稀有度：1普通 2稀有 3史诗 4传说")]
    public int BadgeRarity { get; set; } = 1;

    [Column("UNLOCK_CONDITION")]
    [SwaggerSchema("解锁条件JSON")]
    public string? UnlockCondition { get; set; }

    [Column("IS_ACTIVE")]
    [SwaggerSchema("是否激活")]
    public int IsActive { get; set; } = 1;

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    public ICollection<UserBadgeRelation> UserBadgeRelations { get; set; } = new HashSet<UserBadgeRelation>();
}

