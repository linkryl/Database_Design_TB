/*
 * Project Name:  DatabaseWebAPI
 * File Name:     PasswordUtils.cs
 * File Function: 密码工具函数
  * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.Security.Cryptography;
using System.Text;

namespace DatabaseWebAPI.Utils;

public static class PasswordUtils
{
    // 明文密码转换为哈希密码
    // ReSharper disable once InconsistentNaming
    public static string PlainPasswordToHashedPassword(string plainPassword)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainPassword));
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }
}