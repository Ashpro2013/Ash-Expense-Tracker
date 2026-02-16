using ExpenseIncomeTracker.Web.Models;
using MongoDB.Driver;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class FinanceEntryService
{
    private readonly IMongoCollection<FinanceEntry> _entries;

    public FinanceEntryService(IMongoDatabase database)
    {
        _entries = database.GetCollection<FinanceEntry>("finance_entries");
    }

    public async Task<List<FinanceEntry>> GetAllAsync(string userId)
    {
        var filter = Builders<FinanceEntry>.Filter.Eq(entry => entry.UserId, userId);
        return await MongoAuthGuard.RunAsync(async () =>
            await _entries.Find(filter)
                .SortByDescending(entry => entry.EntryDate)
                .ToListAsync());
    }

    public async Task<List<FinanceEntry>> GetByTypeAsync(string userId, FinanceEntryType type)
    {
        var filter = Builders<FinanceEntry>.Filter.Eq(entry => entry.UserId, userId)
            & Builders<FinanceEntry>.Filter.Eq(entry => entry.Type, type);

        return await MongoAuthGuard.RunAsync(async () =>
            await _entries.Find(filter)
                .SortByDescending(entry => entry.EntryDate)
                .ToListAsync());
    }

    public async Task CreateAsync(FinanceEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        await MongoAuthGuard.RunAsync(async () => await _entries.InsertOneAsync(entry));
    }

    public async Task UpdateAsync(FinanceEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<FinanceEntry>.Filter.Eq(e => e.Id, entry.Id)
            & Builders<FinanceEntry>.Filter.Eq(e => e.UserId, entry.UserId);

        await MongoAuthGuard.RunAsync(async () =>
            await _entries.ReplaceOneAsync(filter, entry, new ReplaceOptions { IsUpsert = false }));
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var filter = Builders<FinanceEntry>.Filter.Eq(e => e.Id, id)
            & Builders<FinanceEntry>.Filter.Eq(e => e.UserId, userId);

        await MongoAuthGuard.RunAsync(async () => await _entries.DeleteOneAsync(filter));
    }
}
