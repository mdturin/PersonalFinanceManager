namespace PersonalFinanceManager.Application.DTOs.Budget;

public class BudgetUpsertDto
{
    public string CategoryId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Month { get; set; } = string.Empty;
}
