using PersonalFinanceManager.Application.DTOs;

namespace PersonalFinanceManager.Application.Interfaces;

/// <summary>
/// Interface for user management service
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> AddToRoleAsync(string userId, string role);
    Task<bool> RemoveFromRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
}
