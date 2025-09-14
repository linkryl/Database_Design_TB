/*
 * Project Name:  DatabaseWebAPI
 * File Name:     AdminLoginRequest.cs
 * File Function: 管理员登录请求模型
 * Author:        TreeHole开发组
 * Update Date:   2025-01-27
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.RequestModels;

[SwaggerSchema(Description = "管理员登录请求")]
public sealed class AdminLoginRequest
{
    [Required]
    [SwaggerSchema("管理员用户名")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [SwaggerSchema("管理员密码")]
    public string Password { get; set; } = string.Empty;
}

