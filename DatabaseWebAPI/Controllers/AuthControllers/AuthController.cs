/*
 * Project Name:  DatabaseWebAPI
 * File Name:     AuthController.cs
 * File Function: 登陆鉴权控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Models.TableModels;
using DatabaseWebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace DatabaseWebAPI.Controllers.AuthControllers;

[ApiController]
[Route("api")]
[SwaggerTag("登陆鉴权相关 API")]
public class AuthController(OracleDbContext context) : ControllerBase
{
    // 获取 Jwt Token
    [HttpPost("get-jwt-token")]
    [SwaggerOperation(Summary = "获取 Jwt Token", Description = "获取 Jwt Token")]
    [SwaggerResponse(200, "登陆鉴权成功")]
    [SwaggerResponse(401, "登陆鉴权失败")]
    public async Task<IActionResult> GetJwtToken([FromBody] PasswordVerificationRequest passwordVerificationRequest)
    {
        if (passwordVerificationRequest.UserId == 0 || string.IsNullOrEmpty(passwordVerificationRequest.PlainPassword))
        {
            return BadRequest(ModelState);
        }

        var user = await context.UserSet.FindAsync(passwordVerificationRequest.UserId);
        if (user == null)
        {
            return Unauthorized();
        }

        var hashedPassword = PasswordUtils.PlainPasswordToHashedPassword(passwordVerificationRequest.PlainPassword);
        if (hashedPassword != user.Password)
        {
            return Unauthorized();
        }

        var token = JwtTokenUtils.GenerateJwtToken(user);
        return Ok(new { token });
    }

    // 管理员登录接口
    [HttpPost("admin-login")]
    [SwaggerOperation(Summary = "管理员登录", Description = "管理员通过固定账号密码登录")]
    [SwaggerResponse(200, "管理员登录成功")]
    [SwaggerResponse(401, "管理员登录失败")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest adminLoginRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(adminLoginRequest.Username) || string.IsNullOrEmpty(adminLoginRequest.Password))
            {
                return BadRequest("用户名和密码不能为空");
            }

            // 固定的管理员账号密码
            const string adminUsername = "admin";
            const string adminPassword = "admin123";

            if (adminLoginRequest.Username != adminUsername || adminLoginRequest.Password != adminPassword)
            {
                return Unauthorized("管理员账号或密码错误");
            }

            // 查找或创建管理员用户
            var adminUser = await context.UserSet.FirstOrDefaultAsync(u => u.UserName == adminUsername);
            if (adminUser == null)
            {
                // 创建管理员用户
                adminUser = new User
                {
                    UserName = adminUsername,
                    Password = PasswordUtils.PlainPasswordToHashedPassword(adminPassword),
                    RegistrationDate = DateTime.Now,
                    LastLoginTime = DateTime.Now,
                    Role = 1, // 1表示管理员角色
                    Status = 1, // 1表示活跃状态
                    Gender = 0,
                    Birthdate = DateTime.Now.AddYears(-25),
                    ExperiencePoints = 0,
                    FollowsCount = 0,
                    FollowedCount = 0,
                    FavoritesCount = 0,
                    FavoritedCount = 0,
                    LikesCount = 0,
                    LikedCount = 0,
                    MessageCount = 0
                };
                context.UserSet.Add(adminUser);
                await context.SaveChangesAsync();
            }
            else
            {
                // 更新最后登录时间
                adminUser.LastLoginTime = DateTime.Now;
                await context.SaveChangesAsync();
            }

            // 生成JWT Token
            var token = JwtTokenUtils.GenerateJwtToken(adminUser);
            
            return Ok(new { 
                token, 
                userId = adminUser.UserId,
                username = adminUser.UserName,
                role = adminUser.Role
            });
        }
        catch (Exception ex)
        {
            // 记录详细错误信息
            Console.WriteLine($"管理员登录错误: {ex.Message}");
            Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            
            return StatusCode(500, new { 
                message = "服务器内部错误", 
                error = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }
}