using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data.Configurations;

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