/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserMessage.cs
 * File Function: UserMessage 模型类
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace DatabaseWebAPI.Models.TableModels;

[Table("USER_MESSAGE")]
[SwaggerSchema(Description = "用户留言表")]
public sealed class UserMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("MESSAGE_ID")]
    [SwaggerSchema("消息ID")]
    public int MessageId { get; set; }

    [Required]
    [Column("SENDER_ID")]
    [SwaggerSchema("发送者ID")]
    public int SenderId { get; set; }

    [Required]
    [Column("RECEIVER_ID")]
    [SwaggerSchema("接收者ID")]
    public int ReceiverId { get; set; }

    [Required]
    [Column("CONTENT")]
    [StringLength(1000)]
    [SwaggerSchema("消息内容")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Column("SEND_TIME")]
    [SwaggerSchema("发送时间")]
    public DateTime SendTime { get; set; }

    [Required]
    [Column("STATUS")]
    [SwaggerSchema("消息状态 0未读 1已读")]
    public int Status { get; set; } = 0;
}