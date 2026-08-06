using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookLoan> BookLoans => Set<BookLoan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();
    public DbSet<WishlistEntry> WishlistEntries => Set<WishlistEntry>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BookEmbedding> BookEmbeddings => Set<BookEmbedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
