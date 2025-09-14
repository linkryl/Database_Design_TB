/*
 * Project Name:  DatabaseWebAPI
 * File Name:     BanUserRequest.cs
 * File Function: 封禁用户请求模型
 * Author:        TreeHole开发组
 * Update Date:   2025-01-27
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.RequestModels;

[SwaggerSchema(Description = "封禁用户请求")]
public sealed class BanUserRequest
{
    [Required]
    [StringLength(500)]
    [SwaggerSchema("封禁原因")]
    public string Reason { get; set; } = string.Empty;
}
