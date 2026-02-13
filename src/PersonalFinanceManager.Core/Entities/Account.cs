using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

public class Account : BaseEntity
{
    public string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Institution { get; set; } = string.Empty;
    public double CurrentBalance { get; set; }
    public string Currency { get; set; } = "BDT";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IncludeInNetWorth { get; set; } = true;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> TransferToTransactions { get; set; } = new List<Transaction>();
}