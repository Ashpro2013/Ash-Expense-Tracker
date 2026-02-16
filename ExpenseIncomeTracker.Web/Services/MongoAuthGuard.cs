using MongoDB.Driver;

namespace ExpenseIncomeTracker.Web.Services;

internal static class MongoAuthGuard
{
    private const string AuthHint = "MongoDB rejected this operation because authentication is required. Configure MongoDb:Username, MongoDb:Password, and MongoDb:AuthDatabase (usually 'admin') in appsettings or environment variables.";

    public static async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (MongoCommandException ex) when (IsAuthFailure(ex))
        {
            throw new InvalidOperationException(AuthHint, ex);
        }
    }

    public static async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (MongoCommandException ex) when (IsAuthFailure(ex))
        {
            throw new InvalidOperationException(AuthHint, ex);
        }
    }

    private static bool IsAuthFailure(MongoCommandException ex)
    {
        if (ex.Code == 13)
        {
            return true;
        }

        return ex.Message.Contains("requires authentication", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase);
    }
}
