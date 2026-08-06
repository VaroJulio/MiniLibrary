using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class BookEmbeddingConfiguration : IEntityTypeConfiguration<BookEmbedding>
{
    public void Configure(EntityTypeBuilder<BookEmbedding> builder)
    {
        builder.ToTable("BookEmbeddings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Vector)
            .IsRequired();

        builder.Property(e => e.GeneratedAt)
            .IsRequired();

        // One-to-one relationship with Book
        builder.HasOne(e => e.Book)
            .WithOne(b => b.Embedding)
            .HasForeignKey<BookEmbedding>(e => e.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
