/*
 * Project Name:  DatabaseWebAPI
 * File Name:     SearchRequest.cs
 * File Function: 搜索请求类
 * Author:        TreeHole开发组
 * Update Date:   2025-07-30
 * License:       Creative Commons Attribution 4.0 International License
 */

using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.RequestModels;

[SwaggerSchema(Description = "帖子搜索请求类")]
public class PostSearchRequest
{
    [SwaggerSchema("帖子ID")] public int PostId { get; set; }
    [SwaggerSchema("标题")] public string Title { get; set; } = string.Empty;
    [SwaggerSchema("内容")] public string Content { get; set; } = string.Empty;
}

[SwaggerSchema(Description = "帖子评论搜索请求类")]
public class PostCommentSearchRequest
{
    [SwaggerSchema("帖子ID")] public int PostId { get; set; }
    [SwaggerSchema("标题")] public string Title { get; set; } = string.Empty;
    [SwaggerSchema("评论内容")] public string Content { get; set; } = string.Empty;
}
