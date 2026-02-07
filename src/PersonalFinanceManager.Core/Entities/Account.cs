using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

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