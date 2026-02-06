using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;
using ExpenseIncomeTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Type).HasConversion<int>();
            entity.Property(c => c.UserId).IsRequired();
            entity.HasIndex(c => new { c.UserId, c.Type, c.Name }).IsUnique();
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Note).HasMaxLength(500);
            entity.Property(t => t.UserId).IsRequired();
            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
