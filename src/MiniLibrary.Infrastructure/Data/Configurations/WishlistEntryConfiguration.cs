using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class WishlistEntryConfiguration : IEntityTypeConfiguration<WishlistEntry>
{
    public void Configure(EntityTypeBuilder<WishlistEntry> builder)
    {
        builder.ToTable("WishlistEntries");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.AddedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne(w => w.Book)
            .WithMany(b => b.WishlistEntries)
            .HasForeignKey(w => w.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.User)
            .WithMany(u => u.WishlistEntries)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate wishlist entries
        builder.HasIndex(w => new { w.UserId, w.BookId })
            .IsUnique()
            .HasDatabaseName("IX_Wishlist_UserId_BookId");

        // Indexes
        builder.HasIndex(w => w.UserId)
            .HasDatabaseName("IX_Wishlist_UserId");

        builder.HasIndex(w => w.BookId)
            .HasDatabaseName("IX_Wishlist_BookId");
    }
}
