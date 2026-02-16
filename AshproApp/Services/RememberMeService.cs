using System.Text.Json;

namespace AshproApp.Services;

public sealed class RememberMeService
{
    private readonly string _sessionFilePath;

    public RememberMeService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = Path.Combine(localAppData, "AshproApp");
        Directory.CreateDirectory(dataDirectory);
        _sessionFilePath = Path.Combine(dataDirectory, "session.json");
    }

    public async Task<bool> SaveAsync(int userId, string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = new RememberedSession(userId, email.Trim(), DateTime.UtcNow);
            await using var stream = File.Create(_sessionFilePath);
            await JsonSerializer.SerializeAsync(stream, session, cancellationToken: cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RememberedSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_sessionFilePath))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(_sessionFilePath);
            return await JsonSerializer.DeserializeAsync<RememberedSession>(stream, cancellationToken: cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public bool Clear()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
            {
                File.Delete(_sessionFilePath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public sealed record RememberedSession(int UserId, string Email, DateTime RememberedAtUtc);
}
