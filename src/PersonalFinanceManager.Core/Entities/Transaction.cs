using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

public class Transaction : BaseEntity
{
    public string UserId { get; set; }
    public string AccountId { get; set; }
    public string? CategoryId { get; set; }
    public string? TransferToAccountId { get; set; } // For transfers
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Reference { get; set; } // Check number, invoice number, etc.
    public bool IsRecurring { get; set; } = false;
    public string? RecurringTransactionId { get; set; }
    public string? Tags { get; set; } // Comma-separated tags

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual Account? TransferToAccount { get; set; }
    public virtual RecurringTransaction? RecurringTransaction { get; set; }
}