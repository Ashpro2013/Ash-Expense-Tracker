using AshproApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<ActivityItem> ActivityItems => Set<ActivityItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(user => user.Email).HasMaxLength(180).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.PasswordSalt).HasMaxLength(200).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<FinanceEntry>(entity =>
        {
            entity.Property(entry => entry.Title).HasMaxLength(140).IsRequired();
            entity.Property(entry => entry.Amount).HasPrecision(18, 2);
            entity.Property(entry => entry.Note).HasMaxLength(500);
            entity.Property(entry => entry.Type).HasConversion<int>();
            entity.Property(entry => entry.UserId).IsRequired();
            entity.HasIndex(entry => new { entry.UserId, entry.Type, entry.EntryDate });
        });

        modelBuilder.Entity<DiaryEntry>(entity =>
        {
            entity.Property(entry => entry.Title).HasMaxLength(180).IsRequired();
            entity.Property(entry => entry.Content).HasMaxLength(5000).IsRequired();
            entity.Property(entry => entry.TagsCsv).HasMaxLength(500);
            entity.Property(entry => entry.UserId).IsRequired();
            entity.HasIndex(entry => new { entry.UserId, entry.EntryDate });
        });

        modelBuilder.Entity<ActivityItem>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(2000);
            entity.Property(item => item.Category).HasMaxLength(120);
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.UserId).IsRequired();
            entity.HasIndex(item => new { item.UserId, item.Status, item.DueDate });
        });
    }
}
