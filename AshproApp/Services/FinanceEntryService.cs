using AshproApp.Data;
using AshproApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Services;

public sealed class FinanceEntryService
{
    private readonly AppDbContextFactory _dbContextFactory;

    public FinanceEntryService(AppDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<FinanceEntry>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        return await db.FinanceEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FinanceEntry>> GetByTypeAsync(int userId, FinanceEntryType type, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        return await db.FinanceEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId && entry.Type == type)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(int userId, FinanceEntry entry, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        entry.UserId = userId;
        entry.CreatedAt = DateTime.UtcNow;
        db.FinanceEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(int userId, FinanceEntry entry, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.FinanceEntries
            .FirstOrDefaultAsync(item => item.Id == entry.Id && item.UserId == userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Title = entry.Title;
        existing.Amount = entry.Amount;
        existing.EntryDate = entry.EntryDate;
        existing.Note = entry.Note;
        existing.Type = entry.Type;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.FinanceEntries
            .FirstOrDefaultAsync(item => item.UserId == userId && item.Id == id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        db.FinanceEntries.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
