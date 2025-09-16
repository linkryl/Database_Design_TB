/*
 * Project Name:  DatabaseWebAPI
 * File Name:     OwnedBg.cs
 * File Function: OwnedBg 模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("OWNEDBG")]
[SwaggerSchema(Description = "用户拥有的背景图片表")]
public sealed class OwnedBg
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("OWNED_BG_ID")]
    [SwaggerSchema("拥有背景ID")]
    public int OwnedBgId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("BG_URL")]
    [StringLength(2048)]
    [SwaggerSchema("背景图片链接")]
    public string BgUrl { get; set; } = string.Empty;

    [Required]
    [Column("BG_NAME")]
    [StringLength(128)]
    [SwaggerSchema("背景图片名称")]
    public string BgName { get; set; } = string.Empty;

    [Required]
    [Column("PURCHASE_DATE")]
    [SwaggerSchema("购买日期")]
    public DateTime PurchaseDate { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}