using AshproApp.Data;
using AshproApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Services;

public sealed class DiaryEntryService
{
    private readonly AppDbContextFactory _dbContextFactory;

    public DiaryEntryService(AppDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<DiaryEntry>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        return await db.DiaryEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(int userId, DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        entry.UserId = userId;
        entry.CreatedAt = DateTime.UtcNow;
        db.DiaryEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(int userId, DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.DiaryEntries
            .FirstOrDefaultAsync(item => item.Id == entry.Id && item.UserId == userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Title = entry.Title;
        existing.Content = entry.Content;
        existing.EntryDate = entry.EntryDate;
        existing.TagsCsv = entry.TagsCsv;
        existing.Mood = entry.Mood;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var existing = await db.DiaryEntries
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        db.DiaryEntries.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
