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