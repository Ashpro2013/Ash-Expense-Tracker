using AshproApp.Data;
using AshproApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Services;

public sealed class ActivityItemService
{
    private readonly AppDbContextFactory _dbContextFactory;

    public ActivityItemService(AppDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<ActivityItem>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        return await db.ActivityItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Status)
            .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(int userId, ActivityItem item, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        item.UserId = userId;
        item.CreatedAt = DateTime.UtcNow;
        db.ActivityItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(int userId, ActivityItem item, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.ActivityItems
            .FirstOrDefaultAsync(entry => entry.Id == item.Id && entry.UserId == userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Category = item.Category;
        existing.Status = item.Status;
        existing.StartDate = item.StartDate;
        existing.DueDate = item.DueDate;
        existing.IsImportant = item.IsImportant;
        existing.CompletedAt = item.Status == ActivityStatus.Done
            ? item.CompletedAt ?? DateTime.UtcNow
            : null;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.ActivityItems
            .FirstOrDefaultAsync(entry => entry.Id == id && entry.UserId == userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        db.ActivityItems.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
