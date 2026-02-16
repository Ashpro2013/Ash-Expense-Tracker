using ExpenseIncomeTracker.Web.Models;
using MongoDB.Driver;

namespace ExpenseIncomeTracker.Web.Services;

public sealed class ActivityService
{
    private readonly IMongoCollection<ActivityItem> _activities;

    public ActivityService(IMongoDatabase database)
    {
        _activities = database.GetCollection<ActivityItem>("activities");
    }

    public async Task<List<ActivityItem>> GetAllAsync(string userId)
    {
        var filter = Builders<ActivityItem>.Filter.Eq(activity => activity.UserId, userId);
        return await MongoAuthGuard.RunAsync(async () =>
            await _activities.Find(filter)
                .SortByDescending(activity => activity.DueDate)
                .ToListAsync());
    }

    public async Task<List<ActivityItem>> GetUpcomingAsync(string userId, int limit)
    {
        var filter = Builders<ActivityItem>.Filter.Eq(activity => activity.UserId, userId)
            & Builders<ActivityItem>.Filter.Ne(activity => activity.Status, ActivityStatus.Done);
        return await MongoAuthGuard.RunAsync(async () =>
            await _activities.Find(filter)
                .SortBy(activity => activity.DueDate)
                .Limit(limit)
                .ToListAsync());
    }

    public async Task CreateAsync(ActivityItem item)
    {
        item.CreatedAt = DateTime.UtcNow;
        await MongoAuthGuard.RunAsync(async () => await _activities.InsertOneAsync(item));
    }

    public async Task UpdateAsync(ActivityItem item)
    {
        if (item.Status == ActivityStatus.Done && item.CompletedAt is null)
        {
            item.CompletedAt = DateTime.UtcNow;
        }

        var filter = Builders<ActivityItem>.Filter.Eq(activity => activity.Id, item.Id)
            & Builders<ActivityItem>.Filter.Eq(activity => activity.UserId, item.UserId);
        await MongoAuthGuard.RunAsync(async () =>
            await _activities.ReplaceOneAsync(filter, item, new ReplaceOptions { IsUpsert = false }));
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var filter = Builders<ActivityItem>.Filter.Eq(activity => activity.Id, id)
            & Builders<ActivityItem>.Filter.Eq(activity => activity.UserId, userId);
        await MongoAuthGuard.RunAsync(async () => await _activities.DeleteOneAsync(filter));
    }
}
