namespace ExpenseIncomeTracker.Web.Models;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = "mongodb://localhost:27017";

    public string DatabaseName { get; init; } = "DiaryActivityDb";

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? AuthDatabase { get; init; }

    public string? AuthMechanism { get; init; }
}
