using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.DTOs;

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}