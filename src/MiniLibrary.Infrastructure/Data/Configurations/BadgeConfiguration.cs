using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.ToTable("Badges");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.BadgeType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.EarnedAt)
            .IsRequired();

        // Foreign key
        builder.HasOne(b => b.User)
            .WithMany(u => u.Badges)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Each badge awarded exactly once per member (Requirement 20.2)
        builder.HasIndex(b => new { b.UserId, b.BadgeType })
            .IsUnique()
            .HasDatabaseName("IX_Badges_UserId_BadgeType");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Badges_UserId");
    }
}
