using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.Interfaces;

namespace PersonalFinanceManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService AuthService) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await AuthService.RegisterAsync(registerDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await AuthService.LoginAsync(loginDto);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid access token."
            });
        }

        var accessToken = string.Empty;
        var authHeader = Request.Headers.Authorization.FirstOrDefault();

        if (authHeader != null && authHeader.StartsWith("Bearer "))
            accessToken = authHeader["Bearer ".Length..];

        var result = await AuthService.RefreshTokenAsync(new RefreshTokenDto() { 
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await AuthService.LogoutAsync(userId);
        var message = result
            ? "Logged out successfully"
            : "Logout failed";

        return result
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await AuthService.ChangePasswordAsync(userId, changePasswordDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await AuthService.ForgotPasswordAsync(forgotPasswordDto);
        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await AuthService.ResetPasswordAsync(resetPasswordDto);
        
        if (!result)
        {
            return BadRequest(new { message = "Password reset failed" });
        }

        return Ok(new { message = "Password reset successfully" });
    }
}
