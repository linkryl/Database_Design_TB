/*
 * File Function: 贴吧选举请求模型
 * Author:        TreeHole开发组
 */

using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.RequestModels;

// 创建选举请求：由吧主或管理员发起一场新选举
[SwaggerSchema(Description = "创建选举请求")]
public class CreateElectionRequest
{
    [SwaggerSchema("开始时间")] public DateTime StartTime { get; set; }
    [SwaggerSchema("结束时间")] public DateTime EndTime { get; set; }
}

// 报名参选请求：普通用户提交竞选宣言后成为候选人
[SwaggerSchema(Description = "报名参选请求")]
public class JoinElectionRequest
{
    [SwaggerSchema("竞选宣言")] public string? Manifesto { get; set; }
}

// 投票请求：对指定候选人投票（同一场选举中一人仅能投一次）
[SwaggerSchema(Description = "投票请求")]
public class VoteElectionRequest
{
    [SwaggerSchema("候选人用户ID")] public int CandidateUserId { get; set; }
}


