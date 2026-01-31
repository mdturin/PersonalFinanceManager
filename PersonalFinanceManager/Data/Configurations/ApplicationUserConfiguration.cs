using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.Currency)
            .HasMaxLength(3);

        builder.Property(u => u.TimeZone)
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.UserName)
            .IsUnique();
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.CurrentBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.InitialBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.Currency)
                .HasMaxLength(3);

            builder.Property(a => a.Description)
                .HasMaxLength(500);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => new { a.UserId, a.IsActive });
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => new { c.UserId, c.Type });
        }
    }

    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.Notes)
                .HasMaxLength(1000);

            builder.Property(t => t.Reference)
                .HasMaxLength(100);

            builder.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.TransferToAccount)
                .WithMany(a => a.TransferToTransactions)
                .HasForeignKey(t => t.TransferToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.AccountId);
            builder.HasIndex(t => t.CategoryId);
            builder.HasIndex(t => t.Date);
            builder.HasIndex(t => new { t.AccountId, t.Date });
            builder.HasIndex(t => new { t.AccountId, t.Type });
        }
    }

    public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
    {
        public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
        {
            builder.ToTable("RecurringTransactions");
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(rt => rt.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rt => rt.Account)
                .WithMany()
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => new { rt.UserId, rt.IsActive });
        }
    }

    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.AlertThreshold)
                .HasColumnType("decimal(5,2)");

            builder.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => new { b.UserId, b.IsActive });
        }
    }

    public class GoalConfiguration : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> builder)
        {
            builder.ToTable("Goals");
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(g => g.Description)
                .HasMaxLength(1000);

            builder.Property(g => g.TargetAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.CurrentAmount)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(g => g.UserId);
            builder.HasIndex(g => new { g.UserId, g.Status });
        }
    }