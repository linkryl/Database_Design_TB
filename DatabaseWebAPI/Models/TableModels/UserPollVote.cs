/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserPollVote.cs
 * File Function: 用户投票记录模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_POLL_VOTE")]
[SwaggerSchema(Description = "用户投票记录表")]
public sealed class UserPollVote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("VOTE_ID")]
    [SwaggerSchema("投票记录ID")]
    public int VoteId { get; set; }

    [Required]
    [Column("POLL_ID")]
    [SwaggerSchema("投票ID")]
    public int PollId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("投票用户ID")]
    public int UserId { get; set; }

    [Required]
    [Column("OPTION_ID")]
    [SwaggerSchema("选择的选项ID")]
    public int OptionId { get; set; }

    [Column("VOTE_TIME")]
    [SwaggerSchema("投票时间")]
    public DateTime VoteTime { get; set; } = DateTime.Now;

    [Column("IP_ADDRESS")]
    [StringLength(45)]
    [SwaggerSchema("投票时的IP地址")]
    public string? IpAddress { get; set; }

    [Column("USER_AGENT")]
    [StringLength(500)]
    [SwaggerSchema("用户代理信息")]
    public string? UserAgent { get; set; }

    // 导航属性
    [ForeignKey("PollId")]
    public Poll? Poll { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("OptionId")]
    public PollOption? Option { get; set; }
}
