/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PostCategory.cs
 * File Function: PostCategory 模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("POST_CATEGORY")]
[SwaggerSchema(Description = "帖子分类表")]
public sealed class PostCategory
{
    // 属性定义
    [Key]
    [Column("CATEGORY_ID")]
    [SwaggerSchema("帖子分类ID")]
    public int CategoryId { get; set; }

    [Required]
    [Column("CATEGORY")]
    [StringLength(64)]
    [SwaggerSchema("帖子分类")]
    public string Category { get; set; } = string.Empty;

    // 导航属性
    public ICollection<Post> PostEntity { get; set; } = new HashSet<Post>();
}