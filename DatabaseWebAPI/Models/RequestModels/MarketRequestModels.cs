/*
 * Project Name:  DatabaseWebAPI
 * File Name:     MarketRequestModels.cs
 * File Function: 商城相关请求模型
 * Author:        TreeHole开发组
 * Update Date:   2025-09-15
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.ComponentModel.DataAnnotations;

namespace DatabaseWebAPI.Models.RequestModels;

/// <summary>
/// 购买背景请求模型
/// </summary>
public class BuyBgRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 背景图片链接
    /// </summary>
    [Required]
    [StringLength(2048)]
    public string BgUrl { get; set; } = string.Empty;

    /// <summary>
    /// 背景图片名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string BgName { get; set; } = string.Empty;
}

/// <summary>
/// 购买头像框请求模型
/// </summary>
public class BuyFrameRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 头像框颜色
    /// </summary>
    [Required]
    [StringLength(32)]
    public string FrameColor { get; set; } = string.Empty;

    /// <summary>
    /// 头像框名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string FrameName { get; set; } = string.Empty;
}

/// <summary>
/// 更新背景请求模型
/// </summary>
public class UpdateBgRequest
{
    /// <summary>
    /// 背景图片链接
    /// </summary>
    [Required]
    [StringLength(2048)]
    public string BgUrl { get; set; } = string.Empty;

    /// <summary>
    /// 背景图片名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string BgName { get; set; } = string.Empty;
}

/// <summary>
/// 更新头像框请求模型
/// </summary>
public class UpdateFrameRequest
{
    /// <summary>
    /// 头像框名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string FrameName { get; set; } = string.Empty;

    /// <summary>
    /// 头像框颜色
    /// </summary>
    [Required]
    [StringLength(32)]
    public string FrameColor { get; set; } = string.Empty;
}

/// <summary>
/// 更新金币数量请求模型
/// </summary>
public class UpdateCoinRequest
{
    /// <summary>
    /// 金币数量
    /// </summary>
    [Required]
    public int CoinCount { get; set; }
}