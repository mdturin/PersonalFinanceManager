using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.DTOs.Account;

public class AccountDto
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Institution { get; set; } = string.Empty;
    public double CurrentBalance { get; set; }
    public string Currency { get; set; } = "BDT";
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}