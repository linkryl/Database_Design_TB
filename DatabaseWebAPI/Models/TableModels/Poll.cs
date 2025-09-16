/*
 * Project Name:  DatabaseWebAPI
 * File Name:     Poll.cs
 * File Function: 投票模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("POLL")]
[SwaggerSchema(Description = "投票表")]
public sealed class Poll
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("POLL_ID")]
    [SwaggerSchema("投票ID")]
    public int PollId { get; set; }

    [Required]
    [Column("POST_ID")]
    [SwaggerSchema("关联帖子ID")]
    public int PostId { get; set; }

    [Required]
    [Column("POLL_TITLE")]
    [StringLength(200)]
    [SwaggerSchema("投票标题")]
    public string PollTitle { get; set; } = string.Empty;

    [Column("POLL_DESCRIPTION")]
    [StringLength(500)]
    [SwaggerSchema("投票描述")]
    public string? PollDescription { get; set; }

    [Required]
    [Column("POLL_TYPE")]
    [SwaggerSchema("投票类型：1单选 2多选")]
    public int PollType { get; set; } = 1;

    [Column("ALLOW_CHANGE_VOTE")]
    [SwaggerSchema("是否允许修改投票")]
    public int AllowChangeVote { get; set; } = 1;

    [Column("SHOW_RESULTS")]
    [SwaggerSchema("是否显示结果")]
    public int ShowResults { get; set; } = 1;

    [Column("DEADLINE_TIME")]
    [SwaggerSchema("截止时间")]
    public DateTime? DeadlineTime { get; set; }

    [Column("TOTAL_VOTES")]
    [SwaggerSchema("总投票数")]
    public int TotalVotes { get; set; } = 0;

    [Column("IS_ACTIVE")]
    [SwaggerSchema("是否激活")]
    public int IsActive { get; set; } = 1;

    [Column("CREATED_TIME")]
    [SwaggerSchema("创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("PostId")]
    public Post? Post { get; set; }

    public ICollection<PollOption> Options { get; set; } = new HashSet<PollOption>();
    public ICollection<UserPollVote> Votes { get; set; } = new HashSet<UserPollVote>();
}
