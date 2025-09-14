/*
 * Project Name:  DatabaseWebAPI
 * File Name:     Bar.cs
 * File Function: Bar 模型类 - 贴吧表
 * Author:        TreeHole开发组
 * Update Date:   2025-09-14
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("BAR")]
[SwaggerSchema(Description = "贴吧表")]
public sealed class Bar
{
    // 属性定义
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("BAR_ID")]
    [SwaggerSchema("贴吧ID")]
    public int BarId { get; set; }

    [Required]
    [Column("OWNER_ID")]
    [SwaggerSchema("吧主ID")]
    public int OwnerId { get; set; }

    [Required]
    [Column("BAR_NAME")]
    [StringLength(128)]
    [SwaggerSchema("贴吧名称")]
    public string BarName { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    [StringLength(1024)]
    [SwaggerSchema("贴吧描述")]
    public string? Description { get; set; }

    [Column("AVATAR_URL")]
    [StringLength(512)]
    [SwaggerSchema("贴吧头像链接")]
    public string? AvatarUrl { get; set; }

    [Column("COVER_URL")]
    [StringLength(512)]
    [SwaggerSchema("贴吧封面链接")]
    public string? CoverUrl { get; set; }

    [Required]
    [Column("CREATION_DATE")]
    [SwaggerSchema("创建时间")]
    public DateTime CreationDate { get; set; }

    [Required]
    [Column("UPDATE_DATE")]
    [SwaggerSchema("更新时间")]
    public DateTime UpdateDate { get; set; }

    [Required]
    [Column("STATUS")]
    [SwaggerSchema("状态：0=正常，1=归档，2=已解散")]
    public int Status { get; set; } = 0;

    [Required]
    [Column("FOLLOWED_COUNT")]
    [SwaggerSchema("关注人数")]
    public int FollowedCount { get; set; } = 0;

    [Required]
    [Column("POST_COUNT")]
    [SwaggerSchema("帖子总数")]
    public int PostCount { get; set; } = 0;

    [Column("RULES")]
    [StringLength(2048)]
    [SwaggerSchema("贴吧规则")]
    public string? Rules { get; set; }

    [Column("TAGS")]
    [StringLength(256)]
    [SwaggerSchema("贴吧标签")]
    public string? Tags { get; set; }

    // 暂时不添加导航属性，避免影响现有功能
}
