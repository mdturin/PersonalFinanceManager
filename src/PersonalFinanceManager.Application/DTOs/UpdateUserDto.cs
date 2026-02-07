using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.DTOs;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [Phone] public string? PhoneNumber { get; set; }
}