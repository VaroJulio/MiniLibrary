using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.ExternalId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailAlertsExpiration)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.EmailAlertsAvailability)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // Soft-delete global query filter (Requirement 11.3)
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Unique email index
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // Unique (ExternalId, Provider) to prevent duplicate SSO users
        builder.HasIndex(u => new { u.ExternalId, u.Provider })
            .IsUnique()
            .HasDatabaseName("IX_Users_ExternalId_Provider");
    }
}
