using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.DTOs.Budget;

public class BudgetDto
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public double Amount { get; set; }
    public string Month { get; set; } = string.Empty;

    public BudgetDto()
    {
    }

    public BudgetDto(Core.Entities.Budget budget)
    {
        Id = budget.Id;
        CategoryId = budget.CategoryId ?? string.Empty;
        CategoryName = budget.Category?.Name;
        Amount = budget.Amount;
        Month = $"{budget.StartDate.Year}-{budget.StartDate.Month:D2}";
    }
}
