using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class BookLoanConfiguration : IEntityTypeConfiguration<BookLoan>
{
    public void Configure(EntityTypeBuilder<BookLoan> builder)
    {
        builder.ToTable("BookLoans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.BorrowedAt)
            .IsRequired();

        builder.Property(l => l.DueDate)
            .IsRequired();

        builder.Property(l => l.ReturnedAt);

        // Foreign keys
        builder.HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.User)
            .WithMany(u => u.Loans)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => new { l.UserId, l.ReturnedAt })
            .HasDatabaseName("IX_BookLoans_UserId_ReturnedAt");

        builder.HasIndex(l => new { l.BookId, l.ReturnedAt })
            .HasDatabaseName("IX_BookLoans_BookId_ReturnedAt");

        builder.HasIndex(l => l.DueDate)
            .HasDatabaseName("IX_BookLoans_DueDate");
    }
}
