using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data.Configurations;

public class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
{
    public void Configure(EntityTypeBuilder<ReviewVote> builder)
    {
        builder.ToTable("ReviewVotes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne(v => v.Rating)
            .WithMany(r => r.Votes)
            .HasForeignKey(v => v.RatingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany(u => u.ReviewVotes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One vote per user per review
        builder.HasIndex(v => new { v.UserId, v.RatingId })
            .IsUnique()
            .HasDatabaseName("IX_ReviewVotes_UserId_RatingId");
    }
}
