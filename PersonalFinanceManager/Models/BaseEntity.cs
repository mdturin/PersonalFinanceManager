using PersonalFinanceManager.Enums;

namespace PersonalFinanceManager.Models;

public class BaseEntity
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Account : BaseEntity
{
    public string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IncludeInNetWorth { get; set; } = true;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> TransferToTransactions { get; set; } = new List<Transaction>();
}

public class Category : BaseEntity
{
    public string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? ParentCategoryId { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsSystem { get; set; } = false; // System categories can't be deleted
    public int SortOrder { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}

public class Transaction : BaseEntity
{
    public string AccountId { get; set; }
    public string? CategoryId { get; set; }
    public string? TransferToAccountId { get; set; } // For transfers
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Reference { get; set; } // Check number, invoice number, etc.
    public bool IsRecurring { get; set; } = false;
    public string? RecurringTransactionId { get; set; }
    public string? Tags { get; set; } // Comma-separated tags

    // Navigation properties
    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual Account? TransferToAccount { get; set; }
    public virtual RecurringTransaction? RecurringTransaction { get; set; }
}

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

public class Budget : BaseEntity
{
    public string UserId { get; set; }
    public string? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool SendAlerts { get; set; } = true;
    public decimal AlertThreshold { get; set; } = 80; // Alert at 80% of budget

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Category? Category { get; set; }
}

public class Goal : BaseEntity
{
    public string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.InProgress;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Priority { get; set; } = 0;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}