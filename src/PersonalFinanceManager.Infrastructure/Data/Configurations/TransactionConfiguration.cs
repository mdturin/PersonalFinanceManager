using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data.Configurations;

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