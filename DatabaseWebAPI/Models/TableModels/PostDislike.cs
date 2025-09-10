/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PostDislike.cs
 * File Function: PostDislike 模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("POST_DISLIKE")]
[SwaggerSchema(Description = "帖子点踩表")]
[PrimaryKey(nameof(PostId), nameof(UserId))]
public sealed class PostDislike
{
    // 属性定义
    [Column("POST_ID")]
    [ForeignKey("Post")]
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }

    [Column("USER_ID")]
    [ForeignKey("User")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("DISLIKE_TIME")]
    [SwaggerSchema("点踩时间")]
    public DateTime DislikeTime { get; set; }

    // 关系定义
    public Post? Post { get; set; }
    public User? User { get; set; }
}