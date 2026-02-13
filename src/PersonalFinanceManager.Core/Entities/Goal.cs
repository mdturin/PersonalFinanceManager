using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

public class Goal : BaseEntity
{
    public string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double TargetAmount { get; set; }
    public double CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.InProgress;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Priority { get; set; } = 0;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}