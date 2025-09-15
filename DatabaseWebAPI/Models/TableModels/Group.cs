/*
 * Project Name:  DatabaseWebAPI
 * File Name:     Group.cs
 * File Function: Group 实体模型
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseWebAPI.Models.TableModels
{
    /// <summary>
    /// 群组实体模型
    /// </summary>
    public class Group
    {
        /// <summary>
        /// 群组ID（主键）
        /// </summary>
        [Key]
        public int GroupId { get; set; }

        /// <summary>
        /// 群组名称
        /// </summary>
        [Required]
        [StringLength(128)]
        public string? GroupName { get; set; }

        /// <summary>
        /// 群组描述
        /// </summary>
        [StringLength(512)]
        public string? GroupDesc { get; set; }

        /// <summary>
        /// 创建者用户ID
        /// </summary>
        [Required]
        public int CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime? LastActiveTime { get; set; }

        /// <summary>
        /// 群组成员数量
        /// </summary>
        public int MemberCount { get; set; } = 0;
    }
}