/*
 * Project Name:  DatabaseWebAPI
 * File Name:     UserController.cs
 * File Function: User 控制器
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using System;
using System.Threading.Tasks;
using DatabaseWebAPI.Data;
using DatabaseWebAPI.Models.RequestModels;
using DatabaseWebAPI.Models.TableModels;
using DatabaseWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DatabaseWebAPI.Controllers.ModelsControllers;

[Route("api/user")]
[ApiController]
[SwaggerTag("用户表相关 API")]
public class UserController(OracleDbContext context) : ControllerBase
{
    // 获取用户表的所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取用户表的所有数据", Description = "获取用户表的所有数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<User>>> GetUser()
    {
        try
        {
            return Ok(await context.UserSet.ToListAsync());
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Database update error: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）获取用户表的数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取用户表的数据", Description = "根据主键（ID）获取用户表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<User>> GetUserByPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）获取用户表的用户名数据
    [HttpGet("user-name/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取用户表的用户名数据", Description = "根据主键（ID）获取用户表的用户名数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<User>> GetUserNameByPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(user.UserName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）获取用户表的头像链接数据
    [HttpGet("avatar-url/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取用户表的头像链接数据", Description = "根据主键（ID）获取用户表的头像链接数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<User>> GetUserAvatarUrlByPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(user.AvatarUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）获取用户表的用户角色数据
    [HttpGet("role/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取用户表的用户角色数据", Description = "根据主键（ID）获取用户表的用户角色数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<User>> GetUserRoleByPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            return Ok(user.Role);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）删除用户表的数据
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除用户表的数据", Description = "根据主键（ID）删除用户表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<User>> DeleteUserByPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            context.UserSet.Remove(user);
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 向用户表添加数据项
    [HttpPost]
    [SwaggerOperation(Summary = "向用户表添加数据项", Description = "向用户表添加数据项（不需要提供 USER_ID，因为它是由系统自动生成的）")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> PostUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.UserSet.Add(user);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostUser), new { id = user.UserId }, user);
    }

    // 根据主键（ID）更新用户表的数据
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新用户表的数据", Description = "根据主键（ID）更新用户表的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.UserId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Entry(user).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.UserSet.Any(e => e.UserId == id))
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return Ok($"Data with ID: {id} has been updated successfully.");
    }

    // 根据主键（ID）更新用户表的个人信息数据
    [HttpPut("personal-information/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新用户表的个人信息数据", Description = "根据主键（ID）更新用户表的个人信息数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdatePersonalInformation(int id,
        [FromBody] PersonalInformationRequest personalInformationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await context.UserSet.FindAsync(id);
        if (user == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }

        user.Profile = personalInformationRequest.Profile;
        user.Gender = personalInformationRequest.Gender;
        user.Birthdate = personalInformationRequest.Birthdate;
        try
        {
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）更新用户表的头像链接数据
    [HttpPut("avatar-url/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新用户表的头像链接数据", Description = "根据主键（ID）更新用户表的头像链接数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdateAvatarUrl(int id, [FromBody] AvatarUrlRequest avatarUrlRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await context.UserSet.FindAsync(id);
        if (user == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }

        user.AvatarUrl = avatarUrlRequest.AvatarUrl;
        try
        {
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）更新用户表的上次登录时间数据
    [HttpPut("last-login-time/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新用户表的上次登录时间数据", Description = "根据主键（ID）更新用户表的上次登录时间数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdateLastLoginTime(int id, [FromBody] LastLoginTimeRequest lastLoginTimeRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await context.UserSet.FindAsync(id);
        if (user == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }

        user.LastLoginTime = lastLoginTimeRequest.LastLoginTime;
        try
        {
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键（ID）更新用户表的密码数据
    [HttpPut("password/{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）更新用户表的密码数据", Description = "根据主键（ID）更新用户表的密码数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] PlainPasswordRequest plainPasswordRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await context.UserSet.FindAsync(id);
        if (user == null)
            return NotFound($"No corresponding data found for ID: {id}");

        user.Password = PasswordUtils.PlainPasswordToHashedPassword(plainPasswordRequest.PlainPassword);
        try
        {
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 判断用户名是否存在于用户表中
    [HttpGet("check-username-unique/{username}")]
    [SwaggerOperation(Summary = "判断用户名是否存在于用户表中", Description = "判断用户名是否存在于用户表中")]
    [SwaggerResponse(200, "请求成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<int>> CheckUserNameUnique(string userName)
    {
        try
        {
            var count = await context.UserSet.CountAsync(u => u.UserName == userName);
            return Ok(count > 0 ? 1 : 0);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    // 验证用户密码是否正确
    [HttpPost("verify-password")]
    [SwaggerOperation(Summary = "验证用户密码是否正确", Description = "验证用户密码是否正确")]
    [SwaggerResponse(200, "请求成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<int>> VerifyPassword(
        [FromBody] PasswordVerificationRequest passwordVerificationRequest)
    {
        if (passwordVerificationRequest.UserId == 0 || string.IsNullOrEmpty(passwordVerificationRequest.PlainPassword))
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await context.UserSet.FindAsync(passwordVerificationRequest.UserId);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {passwordVerificationRequest.UserId}");
            }

            var hashedPassword = PasswordUtils.PlainPasswordToHashedPassword(passwordVerificationRequest.PlainPassword);
            return Ok(hashedPassword == user.Password ? 1 : 0);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    // 根据用户名获取用户 ID
    [HttpGet("get-user-id-by-username/{username}")]
    [SwaggerOperation(Summary = "根据用户名获取用户 ID", Description = "根据用户名获取用户 ID")]
    [SwaggerResponse(200, "请求成功")]
    [SwaggerResponse(404, "未找到对应的用户名")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<int>> GetUserIdByUsername(string username)
    {
        try
        {
            var user = await context.UserSet.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound($"No corresponding data found for username: {username}");
            }

            return Ok(user.UserId);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 重置密码：通过用户名与出生日期验证
    [HttpPost("reset-password")] 
    [AllowAnonymous]
    [SwaggerOperation(Summary = "重置密码", Description = "通过用户名与出生日期验证后重置密码")]
    [SwaggerResponse(200, "重置成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "用户不存在或信息不匹配")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
    {
        if (string.IsNullOrWhiteSpace(resetRequest.UserName) || string.IsNullOrWhiteSpace(resetRequest.NewPlainPassword))
        {
            return BadRequest("用户名和新密码不能为空");
        }

        try
        {
            var user = await context.UserSet.FirstOrDefaultAsync(u => u.UserName == resetRequest.UserName);
            if (user == null)
            {
                return NotFound("用户不存在");
            }

            // 仅以生日为密保问题
            if (user.Birthdate.Date != resetRequest.Birthdate.Date)
            {
                return NotFound("用户信息不匹配");
            }

            user.Password = PasswordUtils.PlainPasswordToHashedPassword(resetRequest.NewPlainPassword);
            await context.SaveChangesAsync();

            return Ok(new { message = "密码重置成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 管理员封禁用户接口
    [HttpPost("admin/ban/{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "管理员封禁用户", Description = "只有管理员可以封禁用户")]
    [SwaggerResponse(200, "封禁成功")]
    [SwaggerResponse(401, "未授权")]
    [SwaggerResponse(403, "权限不足")]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> AdminBanUser(int id, [FromBody] BanUserRequest banRequest)
    {
        try
        {
            // 获取当前用户信息
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                             User.FindFirst("userId") ?? 
                             User.FindFirst("sub");
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var adminUserId))
            {
                return Unauthorized("无法获取管理员信息");
            }

            // 检查用户是否为管理员
            var adminUser = await context.UserSet.FindAsync(adminUserId);
            if (adminUser == null || adminUser.Role != 1)
            {
                return Forbid("只有管理员可以封禁用户");
            }

            // 查找要封禁的用户
            var targetUser = await context.UserSet.FindAsync(id);
            if (targetUser == null)
            {
                return NotFound($"用户ID {id} 不存在");
            }

            // 不能封禁管理员
            if (targetUser.Role == 1)
            {
                return BadRequest("不能封禁管理员用户");
            }

            // 封禁用户
            targetUser.Status = 0; // 0表示被封禁状态
            await context.SaveChangesAsync();

            return Ok(new { 
                message = $"用户 {targetUser.UserName} 已被封禁", 
                bannedUser = new { 
                    id = targetUser.UserId, 
                    username = targetUser.UserName,
                    status = targetUser.Status
                },
                reason = banRequest.Reason
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 管理员解封用户接口
    [HttpPost("admin/unban/{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "管理员解封用户", Description = "只有管理员可以解封用户")]
    [SwaggerResponse(200, "解封成功")]
    [SwaggerResponse(401, "未授权")]
    [SwaggerResponse(403, "权限不足")]
    [SwaggerResponse(404, "用户不存在")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> AdminUnbanUser(int id)
    {
        try
        {
            // 获取当前用户信息
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                             User.FindFirst("userId") ?? 
                             User.FindFirst("sub");
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var adminUserId))
            {
                return Unauthorized("无法获取管理员信息");
            }

            // 检查用户是否为管理员
            var adminUser = await context.UserSet.FindAsync(adminUserId);
            if (adminUser == null || adminUser.Role != 1)
            {
                return Forbid("只有管理员可以解封用户");
            }

            // 查找要解封的用户
            var targetUser = await context.UserSet.FindAsync(id);
            if (targetUser == null)
            {
                return NotFound($"用户ID {id} 不存在");
            }

            // 解封用户
            targetUser.Status = 1; // 1表示正常状态
            await context.SaveChangesAsync();

            return Ok(new { 
                message = $"用户 {targetUser.UserName} 已被解封", 
                unbannedUser = new { 
                    id = targetUser.UserId, 
                    username = targetUser.UserName,
                    status = targetUser.Status
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 临时修复接口：更新所有普通用户状态为正常
    [HttpPost("fix-user-status")]
    [SwaggerOperation(Summary = "修复用户状态", Description = "将所有普通用户状态设置为正常")]
    [SwaggerResponse(200, "修复成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> FixUserStatus()
    {
        try
        {
            // 更新所有普通用户（Role = 0）且状态为封禁（Status = 0）的用户
            var usersToUpdate = await context.UserSet
                .Where(u => u.Role == 0 && u.Status == 0)
                .ToListAsync();

            foreach (var user in usersToUpdate)
            {
                user.Status = 1; // 设置为正常状态
            }

            await context.SaveChangesAsync();

            return Ok(new { 
                message = $"已修复 {usersToUpdate.Count} 个用户的状态",
                updatedUsers = usersToUpdate.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据userId更新用户表的金币数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="request">更新金币请求</param>
    /// <returns>更新结果</returns>
    [HttpGet("update-coin-by-user-id/{userId}")]
    [SwaggerOperation(Summary = "根据userId更新用户表的金币数量")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> UpdateCoinByUserId(int userId, [FromQuery] int coinCount)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            user.CoinCount = coinCount;
            await context.SaveChangesAsync();

            return Ok("金币数量更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据userId获取用户信息（商城用）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户信息</returns>
    [HttpGet("user-info-for-market/{userId}")]
    [SwaggerOperation(Summary = "根据userId获取用户信息（商城用）")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> GetUserInfoForMarket(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            var userInfo = new
            {
                username = user.UserName,
                avatarUrl = user.AvatarUrl ?? "",
                frameColor = user.FrameColor ?? "",
                coinCount = user.CoinCount
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新用户背景
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="request">更新背景请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("update-bg-by-user-id/{userId}")]
    [SwaggerOperation(Summary = "更新用户背景")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> UpdateBgByUserId(int userId, [FromBody] UpdateBgRequest request)
    {
        if (userId <= 0 || request == null)
        {
            return BadRequest("用户ID和请求参数不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            user.BgUrl = request.BgUrl;
            user.BgName = request.BgName;
            await context.SaveChangesAsync();

            return Ok("背景更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新用户头像框
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="request">更新头像框请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("update-frame-by-user-id/{userId}")]
    [SwaggerOperation(Summary = "更新用户头像框")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> UpdateFrameByUserId(int userId, [FromBody] UpdateFrameRequest request)
    {
        if (userId <= 0 || request == null)
        {
            return BadRequest("用户ID和请求参数不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            user.FrameName = request.FrameName;
            user.FrameColor = request.FrameColor;
            await context.SaveChangesAsync();

            return Ok("头像框更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户背景信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户背景信息</returns>
    [HttpGet("get-bg-by-user-id/{userId}")]
    [SwaggerOperation(Summary = "获取用户背景信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> GetBgByUserId(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            var bgInfo = new
            {
                bgName = user.BgName ?? "",
                bgUrl = user.BgUrl ?? ""
            };

            return Ok(bgInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户头像框信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户头像框信息</returns>
    [HttpGet("get-frame-by-user-id/{userId}")]
    [SwaggerOperation(Summary = "获取用户头像框信息")]
    [SwaggerResponse(200, "获取成功")]
    [SwaggerResponse(400, "请求参数错误")]
    [SwaggerResponse(404, "用户未找到")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> GetFrameByUserId(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("用户ID不能为空");
        }

        try
        {
            var user = await context.UserSet.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用户未找到");
            }

            var frameInfo = new
            {
                frameName = user.FrameName ?? "",
                frameColor = user.FrameColor ?? ""
            };

            return Ok(frameInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}