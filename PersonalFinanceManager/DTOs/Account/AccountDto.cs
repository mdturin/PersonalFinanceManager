using PersonalFinanceManager.Enums;

namespace PersonalFinanceManager.DTOs.Account;

public class AccountDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IncludeInNetWorth { get; set; } = true;
}

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IncludeInNetWorth { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}