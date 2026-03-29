using ExpenseIncomeTracker.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExpenseIncomeTracker.Web.Persistence;

public sealed class WebAppDbContext : DbContext
{
    public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options)
    {
    }

    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<ActivityItem> ActivityItems => Set<ActivityItem>();
    public DbSet<DayPlanItem> DayPlanItems => Set<DayPlanItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var tagsConverter = new ValueConverter<List<string>, string>(
            tags => string.Join(',', tags),
            raw => string.IsNullOrWhiteSpace(raw)
                ? new List<string>()
                : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());

        var tagsComparer = new ValueComparer<List<string>>(
            (left, right) => left!.SequenceEqual(right!),
            tags => tags.Aggregate(0, (current, tag) => HashCode.Combine(current, tag.GetHashCode(StringComparison.Ordinal))),
            tags => tags.ToList());

        builder.Entity<FinanceEntry>(entity =>
        {
            entity.ToTable("FinanceEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.HasIndex(e => new { e.UserId, e.EntryDate });
        });

        builder.Entity<DiaryEntry>(entity =>
        {
            entity.ToTable("DiaryEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(6000).IsRequired();
            entity.Property(e => e.Tags)
                .HasConversion(tagsConverter)
                .Metadata.SetValueComparer(tagsComparer);
            entity.HasIndex(e => new { e.UserId, e.EntryDate });
        });

        builder.Entity<ActivityItem>(entity =>
        {
            entity.ToTable("ActivityItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(120);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => new { e.UserId, e.Status });
        });

        builder.Entity<DayPlanItem>(entity =>
        {
            entity.ToTable("DayPlanItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.StartTime).HasMaxLength(5).IsRequired();
            entity.Property(e => e.EndTime).HasMaxLength(5).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.UserId, e.PlanDate });
        });
    }
}
