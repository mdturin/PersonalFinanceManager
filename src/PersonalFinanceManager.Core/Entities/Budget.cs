using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

public class Budget : BaseEntity
{
    public string UserId { get; set; }
    public string? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool SendAlerts { get; set; } = true;
    public double AlertThreshold { get; set; } = 80; // Alert at 80% of budget

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Category? Category { get; set; }
}