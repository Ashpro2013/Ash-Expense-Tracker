using ExpenseIncomeTracker.Web.Persistence;
using ExpenseIncomeTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class FinanceEntryService
{
    private readonly WebAppDbContext _db;

    public FinanceEntryService(WebAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FinanceEntry>> GetAllAsync(string userId)
    {
        return await _db.Set<FinanceEntry>()
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<FinanceEntry>> GetByTypeAsync(string userId, FinanceEntryType type)
    {
        return await _db.Set<FinanceEntry>()
            .AsNoTracking()
            .Where(entry => entry.UserId == userId && entry.Type == type)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(FinanceEntry entry)
    {
        entry.Id ??= Guid.NewGuid().ToString("N");
        entry.CreatedAt = DateTime.UtcNow;
        _db.Set<FinanceEntry>().Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(FinanceEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            return;
        }

        var existing = await _db.Set<FinanceEntry>()
            .FirstOrDefaultAsync(e => e.Id == entry.Id && e.UserId == entry.UserId);

        if (existing is null)
        {
            return;
        }

        existing.Title = entry.Title;
        existing.Type = entry.Type;
        existing.Amount = entry.Amount;
        existing.EntryDate = entry.EntryDate;
        existing.Note = entry.Note;
        entry.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedAt = entry.UpdatedAt;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var existing = await _db.Set<FinanceEntry>()
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (existing is null)
        {
            return;
        }

        _db.Set<FinanceEntry>().Remove(existing);
        await _db.SaveChangesAsync();
    }
}
