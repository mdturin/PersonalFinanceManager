using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

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