using ExpenseIncomeTracker.Web.Persistence;
using ExpenseIncomeTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class ActivityService
{
    private readonly WebAppDbContext _db;

    public ActivityService(WebAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ActivityItem>> GetAllAsync(string userId)
    {
        return await _db.Set<ActivityItem>()
            .AsNoTracking()
            .Where(activity => activity.UserId == userId)
            .OrderBy(activity => activity.Status)
            .ThenBy(activity => activity.DueDate ?? DateTime.MaxValue)
            .ToListAsync();
    }

    public async Task<List<ActivityItem>> GetUpcomingAsync(string userId, int limit)
    {
        return await _db.Set<ActivityItem>()
            .AsNoTracking()
            .Where(activity => activity.UserId == userId && activity.Status != ActivityStatus.Done)
            .OrderBy(activity => activity.DueDate ?? DateTime.MaxValue)
            .Take(limit)
            .ToListAsync();
    }

    public async Task CreateAsync(ActivityItem item)
    {
        item.Id ??= Guid.NewGuid().ToString("N");
        item.CreatedAt = DateTime.UtcNow;
        _db.Set<ActivityItem>().Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ActivityItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            return;
        }

        var existing = await _db.Set<ActivityItem>()
            .FirstOrDefaultAsync(activity => activity.Id == item.Id && activity.UserId == item.UserId);

        if (existing is null)
        {
            return;
        }

        if (item.Status == ActivityStatus.Done && item.CompletedAt is null)
        {
            item.CompletedAt = DateTime.UtcNow;
        }

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Category = item.Category;
        existing.Status = item.Status;
        existing.StartDate = item.StartDate;
        existing.DueDate = item.DueDate;
        existing.IsImportant = item.IsImportant;
        existing.CompletedAt = item.CompletedAt;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var existing = await _db.Set<ActivityItem>()
            .FirstOrDefaultAsync(activity => activity.Id == id && activity.UserId == userId);
        if (existing is null)
        {
            return;
        }

        _db.Set<ActivityItem>().Remove(existing);
        await _db.SaveChangesAsync();
    }
}
