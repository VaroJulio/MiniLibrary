using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("Ratings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Score)
            .IsRequired();

        builder.Property(r => r.ReviewText)
            .HasMaxLength(1000);

        builder.Property(r => r.UsefulVotes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne(r => r.Book)
            .WithMany(b => b.Ratings)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(r => r.BookId)
            .HasDatabaseName("IX_Ratings_BookId");

        // One rating per user per book (Requirement 16.3)
        builder.HasIndex(r => new { r.UserId, r.BookId })
            .IsUnique()
            .HasDatabaseName("IX_Ratings_UserId_BookId");
    }
}
