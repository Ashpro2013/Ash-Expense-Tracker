using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExpenseIncomeTracker.Web.Models;

public enum FinanceEntryType
{
    Income,
    Expense
}

public sealed class FinanceEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public FinanceEntryType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTime EntryDate { get; set; } = DateTime.Today;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
