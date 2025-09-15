/*
 * Project Name:  DatabaseWebAPI
 * File Name:     GroupMember.cs
 * File Function: GroupMember 实体模型
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseWebAPI.Models.TableModels
{
    /// <summary>
    /// 群组成员实体模型
    /// </summary>
    public class GroupMember
    {
        /// <summary>
        /// 成员ID（主键）
        /// </summary>
        [Key]
        public int MemberId { get; set; }

        /// <summary>
        /// 群组ID
        /// </summary>
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 加入时间
        /// </summary>
        [Required]
        public DateTime JoinTime { get; set; }

        /// <summary>
        /// 角色（0=普通成员，1=管理员，2=群主）
        /// </summary>
        public int Role { get; set; } = 0;

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMuted { get; set; } = false;
    }
}