using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(13);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.AverageRating)
            .HasColumnType("decimal(3,1)")
            .HasDefaultValue(0m);

        builder.Property(b => b.TotalRatings)
            .HasDefaultValue(0);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // Optimistic concurrency token (Requirement 11.5)
        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Soft-delete global query filter (Requirement 11.3)
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Indexes
        builder.HasIndex(b => b.ISBN)
            .IsUnique()
            .HasDatabaseName("IX_Books_ISBN");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Books_Status");

        builder.HasIndex(b => b.Category)
            .HasDatabaseName("IX_Books_Category");

        builder.HasIndex(b => new { b.Title, b.Author })
            .HasDatabaseName("IX_Books_Title_Author");
    }
}
