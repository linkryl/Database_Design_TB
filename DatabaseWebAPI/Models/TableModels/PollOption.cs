/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PollOption.cs
 * File Function: 投票选项模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("POLL_OPTION")]
[SwaggerSchema(Description = "投票选项表")]
public sealed class PollOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("OPTION_ID")]
    [SwaggerSchema("选项ID")]
    public int OptionId { get; set; }

    [Required]
    [Column("POLL_ID")]
    [SwaggerSchema("投票ID")]
    public int PollId { get; set; }

    [Required]
    [Column("OPTION_TEXT")]
    [StringLength(200)]
    [SwaggerSchema("选项文本")]
    public string OptionText { get; set; } = string.Empty;

    [Column("OPTION_DESCRIPTION")]
    [StringLength(300)]
    [SwaggerSchema("选项描述")]
    public string? OptionDescription { get; set; }

    [Column("VOTE_COUNT")]
    [SwaggerSchema("得票数")]
    public int VoteCount { get; set; } = 0;

    [Column("SORT_ORDER")]
    [SwaggerSchema("排序顺序")]
    public int SortOrder { get; set; } = 0;

    // 导航属性
    [ForeignKey("PollId")]
    public Poll? Poll { get; set; }

    public ICollection<UserPollVote> Votes { get; set; } = new HashSet<UserPollVote>();
}
