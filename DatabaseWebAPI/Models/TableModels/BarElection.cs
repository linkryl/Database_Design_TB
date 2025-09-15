/*
 * File Function: 贴吧选举相关模型
 * Author:        TreeHole开发组
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("BAR_ELECTION")]
[SwaggerSchema(Description = "贴吧选举表")]
public sealed class BarElection
{
    // 选举主表：记录某个贴吧在一段时间内的一场选举
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ELECTION_ID")]
    [SwaggerSchema("选举ID")]
    public int ElectionId { get; set; }

    [Required]
    [Column("BAR_ID")]
    [SwaggerSchema("贴吧ID")]
    public int BarId { get; set; }

    [Required]
    [Column("STATUS")]
    [SwaggerSchema("状态：0未开始 1进行中 2已结束")]
    public int Status { get; set; }

    [Required]
    [Column("START_TIME")]
    [SwaggerSchema("开始时间")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("END_TIME")]
    [SwaggerSchema("结束时间")]
    public DateTime EndTime { get; set; }

    [Required]
    [Column("CREATOR_USER_ID")]
    [SwaggerSchema("创建者用户ID")]
    public int CreatorUserId { get; set; }
}

[Table("BAR_ELECTION_CANDIDATE")]
[SwaggerSchema(Description = "贴吧选举候选人表")]
public sealed class BarElectionCandidate
{
    // 候选人表：记录参与该场选举的用户及其宣言
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CANDIDATE_ID")]
    [SwaggerSchema("候选记录ID")]
    public int CandidateId { get; set; }

    [Required]
    [Column("ELECTION_ID")]
    [SwaggerSchema("选举ID")]
    public int ElectionId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("候选人用户ID")]
    public int UserId { get; set; }

    [Column("MANIFESTO")]
    [StringLength(512)]
    [SwaggerSchema("竞选宣言")]
    public string? Manifesto { get; set; }

    [Column("CREATE_TIME")]
    [SwaggerSchema("报名时间")]
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

[Table("BAR_ELECTION_VOTE")]
[SwaggerSchema(Description = "贴吧选举投票表")]
public sealed class BarElectionVote
{
    // 投票表：记录投票者对某候选人的投票，一人一票（同一场选举）
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("VOTE_ID")]
    [SwaggerSchema("投票记录ID")]
    public int VoteId { get; set; }

    [Required]
    [Column("ELECTION_ID")]
    [SwaggerSchema("选举ID")]
    public int ElectionId { get; set; }

    [Required]
    [Column("VOTER_USER_ID")]
    [SwaggerSchema("投票者用户ID")]
    public int VoterUserId { get; set; }

    [Required]
    [Column("CANDIDATE_USER_ID")]
    [SwaggerSchema("被投票候选人用户ID")]
    public int CandidateUserId { get; set; }

    [Column("VOTE_TIME")]
    [SwaggerSchema("投票时间")]
    public DateTime VoteTime { get; set; } = DateTime.Now;
}


