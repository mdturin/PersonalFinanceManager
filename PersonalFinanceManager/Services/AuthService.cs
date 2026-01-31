using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Configurations;
using PersonalFinanceManager.DTOs;
using PersonalFinanceManager.Interfaces;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Services;

/// <summary>
/// Service for user authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User with this email already exists."
            };
        }

        var user = new ApplicationUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Registration successful.",
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            User = MapToUserDto(user, roles)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        user.LastLoginAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            User = MapToUserDto(user, roles)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        if (principal == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid access token."
            };
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid token claims."
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid or expired refresh token."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            User = MapToUserDto(user, roles)
        };
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return true;
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found."
            };
        }

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password changed successfully."
        };
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // Return true for security reasons (don't reveal if email exists)
            return true;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // TODO: Send email with reset token
        // You should implement email service to send the token to user
        // For now, you might want to log it or return it in development

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager
            .ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        
        return result.Succeeded;
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
