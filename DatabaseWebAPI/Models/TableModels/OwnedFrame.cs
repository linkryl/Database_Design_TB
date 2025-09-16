/*
 * Project Name:  DatabaseWebAPI
 * File Name:     OwnedFrame.cs
 * File Function: OwnedFrame 模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("OWNEDFRAME")]
[SwaggerSchema(Description = "用户拥有的头像框表")]
public sealed class OwnedFrame
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("OWNED_FRAME_ID")]
    [SwaggerSchema("拥有头像框ID")]
    public int OwnedFrameId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("FRAME_COLOR")]
    [StringLength(32)]
    [SwaggerSchema("头像框颜色")]
    public string FrameColor { get; set; } = string.Empty;

    [Required]
    [Column("FRAME_NAME")]
    [StringLength(128)]
    [SwaggerSchema("头像框名称")]
    public string FrameName { get; set; } = string.Empty;

    [Required]
    [Column("PURCHASE_DATE")]
    [SwaggerSchema("购买日期")]
    public DateTime PurchaseDate { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}