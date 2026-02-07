using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.Interfaces;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
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

        var result = await _userService.UpdateUserAsync(userId, updateUserDto);
        
        if (!result)
        {
            return BadRequest(new { message = "Update failed" });
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.UpdateUserAsync(id, updateUserDto);
        
        if (!result)
        {
            return BadRequest(new { message = "Update failed" });
        }

        return Ok(new { message = "User updated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        
        if (!result)
        {
            return BadRequest(new { message = "Delete failed" });
        }

        return Ok(new { message = "User deleted successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        
        if (!result)
        {
            return BadRequest(new { message = "Deactivation failed" });
        }

        return Ok(new { message = "User deactivated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(string id)
    {
        var result = await _userService.ActivateUserAsync(id);
        
        if (!result)
        {
            return BadRequest(new { message = "Activation failed" });
        }

        return Ok(new { message = "User activated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/roles/{role}")]
    public async Task<IActionResult> AddUserToRole(string id, string role)
    {
        var result = await _userService.AddToRoleAsync(id, role);
        
        if (!result)
        {
            return BadRequest(new { message = "Failed to add user to role" });
        }

        return Ok(new { message = $"User added to {role} role successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/roles/{role}")]
    public async Task<IActionResult> RemoveUserFromRole(string id, string role)
    {
        var result = await _userService.RemoveFromRoleAsync(id, role);
        
        if (!result)
        {
            return BadRequest(new { message = "Failed to remove user from role" });
        }

        return Ok(new { message = $"User removed from {role} role successfully" });
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        var roles = await _userService.GetUserRolesAsync(id);
        return Ok(roles);
    }
}