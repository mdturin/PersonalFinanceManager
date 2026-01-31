using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data.Configurations;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data;

/// <summary>
/// Database context for user management
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new RecurringTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new BudgetConfiguration());
        modelBuilder.ApplyConfiguration(new GoalConfiguration());
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set CreatedAt and UpdatedAt
        var entries = ChangeTracker.Entries<BaseEntity>();
            
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}