using Microsoft.EntityFrameworkCore;
using QuotesApi.Models;

namespace QuotesApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>(b =>
        {
            b.HasKey(c => c.Id);
            // EF Core Owned Types mapping for the value object
            b.OwnsMany(c => c.Items, a =>
            {
                a.WithOwner().HasForeignKey("CollectionId");
                a.Property<int>("Id");
                a.HasKey("Id");
            });
        });
    }
}
