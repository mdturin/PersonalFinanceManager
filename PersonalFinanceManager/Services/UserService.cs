using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.DTOs;
using PersonalFinanceManager.Interfaces;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToUserDto(user, roles));
        }

        return userDtos;
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
        {
            user.FirstName = updateUserDto.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
        {
            user.LastName = updateUserDto.LastName;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber))
        {
            user.PhoneNumber = updateUserDto.PhoneNumber;
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> AddToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveFromRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    private UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
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