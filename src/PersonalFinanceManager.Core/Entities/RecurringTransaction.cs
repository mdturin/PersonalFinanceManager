using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

public class RecurringTransaction : BaseEntity
{
    public string UserId { get; set; }
    public string AccountId { get; set; }
    public string? CategoryId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public RecurrenceFrequency Frequency { get; set; }
    public int FrequencyInterval { get; set; } = 1; // e.g., every 2 weeks
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextOccurrence { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}