using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.DTOs.Account;

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Institution { get; set; } = string.Empty;
    public double CurrentBalance { get; set; }
    public string Currency { get; set; } = "BDT";
    public bool IsActive { get; set; } = true;

    public Core.Entities.Account ToAccount(string userId)
    {
        return new Core.Entities.Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = Name,
            Type = Type,
            Institution = Institution,
            CurrentBalance = CurrentBalance,
            Currency = Currency,
            IsActive = IsActive
        };
    }
}