/*
 * Project Name:  DatabaseWebAPI
 * File Name:     JwtTokenUtils.cs
 * File Function: Jwt Token 工具函数
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DatabaseWebAPI.Models.TableModels;
using Microsoft.IdentityModel.Tokens;

namespace DatabaseWebAPI.Utils;

public static class JwtTokenUtils
{
    // Jwt Token 配置项
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json")
        .Build();

    // 生成 Jwt Token
    // ReSharper disable once InconsistentNaming
    public static string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Config["Jwt:Key"] ?? throw new InvalidOperationException()));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("userId", user.UserId.ToString()),
            new Claim("role", user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: Config["Jwt:Issuer"],
            audience: Config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}