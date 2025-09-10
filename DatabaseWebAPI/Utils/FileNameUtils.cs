/*
 * Project Name:  DatabaseWebAPI
 * File Name:     FileNameUtils.cs
 * File Function: 文件名工具函数
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

namespace DatabaseWebAPI.Utils;

public static class FileNameUtils
{
    // 生成随机文件名
    // ReSharper disable once InconsistentNaming
    public static string GenerateRandomFileName(int length)
    {
        var random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}