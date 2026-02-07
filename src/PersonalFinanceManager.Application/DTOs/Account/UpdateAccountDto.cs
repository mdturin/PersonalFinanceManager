namespace PersonalFinanceManager.Application.DTOs.Account;

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IncludeInNetWorth { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}