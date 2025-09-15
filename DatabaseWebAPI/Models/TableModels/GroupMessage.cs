/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupMessage.cs
 * File Function: GroupMessage 实体模型
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseWebAPI.Models.TableModels
{
    /// <summary>
    /// 群组消息实体模型
    /// </summary>
    public class GroupMessage
    {
        /// <summary>
        /// 消息ID（主键）
        /// </summary>
        [Key]
        public int MessageId { get; set; }

        /// <summary>
        /// 群组ID
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// 发送者用户ID
        /// </summary>
        [Required]
        public int SenderId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [StringLength(1024)]
        public string? Content { get; set; }

        /// <summary>
        /// 消息类型（0=文本，1=图片，2=文件）
        /// </summary>
        public int MessageType { get; set; } = 0;

        /// <summary>
        /// 发送时间
        /// </summary>
        [Required]
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 是否已删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 回复的消息ID（可选）- 暂时注释，数据库表中没有此字段
        /// </summary>
        // public int? ReplyToMessageId { get; set; }
    }
}