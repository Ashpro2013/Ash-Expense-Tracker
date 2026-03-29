using ExpenseIncomeTracker.Web.Persistence;
using ExpenseIncomeTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class DiaryService
{
    private readonly WebAppDbContext _db;

    public DiaryService(WebAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DiaryEntry>> GetAllAsync(string userId)
    {
        return await _db.Set<DiaryEntry>()
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<DiaryEntry>> GetRecentAsync(string userId, int limit)
    {
        return await _db.Set<DiaryEntry>()
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task CreateAsync(DiaryEntry entry)
    {
        entry.Id ??= Guid.NewGuid().ToString("N");
        entry.CreatedAt = DateTime.UtcNow;
        _db.Set<DiaryEntry>().Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DiaryEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            return;
        }

        var existing = await _db.Set<DiaryEntry>()
            .FirstOrDefaultAsync(e => e.Id == entry.Id && e.UserId == entry.UserId);

        if (existing is null)
        {
            return;
        }

        existing.Title = entry.Title;
        existing.Content = entry.Content;
        existing.EntryDate = entry.EntryDate;
        existing.Tags = entry.Tags;
        existing.Mood = entry.Mood;

        entry.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedAt = entry.UpdatedAt;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var existing = await _db.Set<DiaryEntry>()
            .FirstOrDefaultAsync(entry => entry.Id == id && entry.UserId == userId);
        if (existing is null)
        {
            return;
        }

        _db.Set<DiaryEntry>().Remove(existing);
        await _db.SaveChangesAsync();
    }
}
