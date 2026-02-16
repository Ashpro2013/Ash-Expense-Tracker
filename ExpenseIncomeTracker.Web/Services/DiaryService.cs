using ExpenseIncomeTracker.Web.Models;
using MongoDB.Driver;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class DiaryService
{
    private readonly IMongoCollection<DiaryEntry> _entries;

    public DiaryService(IMongoDatabase database)
    {
        _entries = database.GetCollection<DiaryEntry>("diary_entries");
    }

    public async Task<List<DiaryEntry>> GetAllAsync(string userId)
    {
        var filter = Builders<DiaryEntry>.Filter.Eq(entry => entry.UserId, userId);
        return await MongoAuthGuard.RunAsync(async () =>
            await _entries.Find(filter)
                .SortByDescending(entry => entry.EntryDate)
                .ToListAsync());
    }

    public async Task<List<DiaryEntry>> GetRecentAsync(string userId, int limit)
    {
        var filter = Builders<DiaryEntry>.Filter.Eq(entry => entry.UserId, userId);
        return await MongoAuthGuard.RunAsync(async () =>
            await _entries.Find(filter)
                .SortByDescending(entry => entry.EntryDate)
                .Limit(limit)
                .ToListAsync());
    }

    public async Task CreateAsync(DiaryEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        await MongoAuthGuard.RunAsync(async () => await _entries.InsertOneAsync(entry));
    }

    public async Task UpdateAsync(DiaryEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<DiaryEntry>.Filter.Eq(e => e.Id, entry.Id)
            & Builders<DiaryEntry>.Filter.Eq(e => e.UserId, entry.UserId);
        await MongoAuthGuard.RunAsync(async () =>
            await _entries.ReplaceOneAsync(filter, entry, new ReplaceOptions { IsUpsert = false }));
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var filter = Builders<DiaryEntry>.Filter.Eq(entry => entry.Id, id)
            & Builders<DiaryEntry>.Filter.Eq(entry => entry.UserId, userId);
        await MongoAuthGuard.RunAsync(async () => await _entries.DeleteOneAsync(filter));
    }
}
