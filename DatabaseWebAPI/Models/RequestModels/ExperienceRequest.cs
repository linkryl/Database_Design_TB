/*
 * Project Name:  DatabaseWebAPI
 * File Name:     ExperienceRequest.cs
 * File Function: 经验值相关请求模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-14
 * License:       Creative Commons Attribution 4.0 International License
 */

using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.RequestModels;

[SwaggerSchema(Description = "经验值增加请求类")]
public class ExperiencePointsRequest
{
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [SwaggerSchema("增加的经验值数量")]
    public int Points { get; set; }
}

[SwaggerSchema(Description = "等级信息响应类")]
public class LevelInfoResponse
{
    [SwaggerSchema("当前等级")]
    public int Level { get; set; }

    [SwaggerSchema("当前经验值")]
    public int CurrentExp { get; set; }

    [SwaggerSchema("升级所需经验值")]
    public int ExpToNextLevel { get; set; }

    [SwaggerSchema("经验值进度百分比")]
    public double ProgressPercentage { get; set; }
}