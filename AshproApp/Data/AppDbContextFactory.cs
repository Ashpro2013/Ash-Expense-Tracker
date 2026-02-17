using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Data;

public sealed class AppDbContextFactory
{
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextFactory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = Path.Combine(localAppData, "AshproApp", "Data");
        Directory.CreateDirectory(dataDirectory);

        var databasePath = Path.Combine(dataDirectory, "ashproapp.db");
        if (File.Exists(databasePath) && NeedsRecreate(databasePath))
        {
            File.Delete(databasePath);
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={databasePath}");
        _options = optionsBuilder.Options;

        EnsureDatabaseCreated(databasePath);
    }

    private void EnsureDatabaseCreated(string databasePath)
    {
        try
        {
            using var db = new AppDbContext(_options);
            db.Database.EnsureCreated();
        }
        catch (SqliteException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }

            using var db = new AppDbContext(_options);
            db.Database.EnsureCreated();
        }
    }

    public AppDbContext CreateDbContext()
    {
        return new AppDbContext(_options);
    }

    private static bool NeedsRecreate(string databasePath)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            return !TableHasColumn(connection, "AppUsers", "Email")
                   || !TableHasColumn(connection, "FinanceEntries", "UserId")
                   || !TableHasColumn(connection, "DiaryEntries", "UserId")
                   || !TableHasColumn(connection, "ActivityItems", "UserId");
        }
        catch
        {
            return true;
        }
    }

    private static bool TableHasColumn(SqliteConnection connection, string tableName, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName.Replace("'", "''")}')";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var existingColumn = reader.GetString(1);
            if (string.Equals(existingColumn, columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
