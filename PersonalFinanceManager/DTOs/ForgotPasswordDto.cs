using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.DTOs;

public class ForgotPasswordDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
}