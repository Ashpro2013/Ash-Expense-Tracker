using ExpenseIncomeTracker.Web.Models;
using ExpenseIncomeTracker.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class DayPlanService
{
    private readonly WebAppDbContext _db;

    public DayPlanService(WebAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DayPlanItem>> GetByDateAsync(string userId, DateTime planDate)
    {
        var date = planDate.Date;
        return await _db.DayPlanItems
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.PlanDate.Date == date)
            .OrderBy(item => item.StartTime)
            .ThenBy(item => item.Title)
            .ToListAsync();
    }

    public async Task CreateAsync(DayPlanItem item)
    {
        item.Id ??= Guid.NewGuid().ToString("N");
        item.CreatedAt = DateTime.UtcNow;
        _db.DayPlanItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DayPlanItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            return;
        }

        var existing = await _db.DayPlanItems
            .FirstOrDefaultAsync(plan => plan.Id == item.Id && plan.UserId == item.UserId);

        if (existing is null)
        {
            return;
        }

        existing.PlanDate = item.PlanDate.Date;
        existing.Title = item.Title;
        existing.StartTime = item.StartTime;
        existing.EndTime = item.EndTime;
        existing.Notes = item.Notes;
        existing.IsCompleted = item.IsCompleted;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ToggleCompletionAsync(string id, string userId)
    {
        var existing = await _db.DayPlanItems
            .FirstOrDefaultAsync(plan => plan.Id == id && plan.UserId == userId);

        if (existing is null)
        {
            return;
        }

        existing.IsCompleted = !existing.IsCompleted;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var existing = await _db.DayPlanItems
            .FirstOrDefaultAsync(plan => plan.Id == id && plan.UserId == userId);

        if (existing is null)
        {
            return;
        }

        _db.DayPlanItems.Remove(existing);
        await _db.SaveChangesAsync();
    }
}
