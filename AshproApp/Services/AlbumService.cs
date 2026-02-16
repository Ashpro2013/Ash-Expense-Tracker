using System.Text.Json;
using AshproApp.Models;

namespace AshproApp.Services;

public sealed class AlbumService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"
    };

    private readonly string _albumsRootDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public AlbumService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _albumsRootDirectory = Path.Combine(localAppData, "AshproApp", "albums");
        Directory.CreateDirectory(_albumsRootDirectory);
    }

    public async Task<List<AlbumImage>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return new List<AlbumImage>();
        }

        var metadataPath = GetMetadataPath(userId);
        if (!File.Exists(metadataPath))
        {
            return new List<AlbumImage>();
        }

        try
        {
            await using var stream = File.OpenRead(metadataPath);
            var list = await JsonSerializer.DeserializeAsync<List<AlbumImage>>(stream, cancellationToken: cancellationToken)
                       ?? new List<AlbumImage>();

            return list
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAtUtc)
                .ToList();
        }
        catch
        {
            return new List<AlbumImage>();
        }
    }

    public async Task<AlbumImage?> AddAsync(
        int userId,
        string sourcePath,
        string category,
        string? caption,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return null;
        }

        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return null;
        }

        var userDirectory = GetUserDirectory(userId);
        Directory.CreateDirectory(userDirectory);

        var id = Guid.NewGuid().ToString("N");
        var fileName = $"{id}{extension.ToLowerInvariant()}";
        var destinationPath = Path.Combine(userDirectory, fileName);

        try
        {
            await using var source = File.OpenRead(sourcePath);
            await using var destination = File.Create(destinationPath);
            await source.CopyToAsync(destination, cancellationToken);

            var image = new AlbumImage
            {
                Id = id,
                UserId = userId,
                Category = NormalizeCategory(category),
                Caption = string.IsNullOrWhiteSpace(caption) ? Path.GetFileNameWithoutExtension(sourcePath) : caption.Trim(),
                FileName = fileName,
                CreatedAtUtc = DateTime.UtcNow
            };

            var all = await GetAllAsync(userId, cancellationToken);
            all.Add(image);
            await SaveAllAsync(userId, all, cancellationToken);

            return image;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int userId, string albumImageId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(albumImageId))
        {
            return false;
        }

        var all = await GetAllAsync(userId, cancellationToken);
        var existing = all.FirstOrDefault(item => item.Id == albumImageId);
        if (existing is null)
        {
            return false;
        }

        all.Remove(existing);
        await SaveAllAsync(userId, all, cancellationToken);

        var fullPath = GetImagePath(userId, existing.FileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return true;
    }

    public string GetImagePath(int userId, string fileName)
    {
        return Path.Combine(GetUserDirectory(userId), fileName);
    }

    private async Task SaveAllAsync(int userId, List<AlbumImage> items, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(userId);
        Directory.CreateDirectory(Path.GetDirectoryName(metadataPath)!);

        await using var stream = File.Create(metadataPath);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions, cancellationToken);
    }

    private string GetUserDirectory(int userId)
    {
        return Path.Combine(_albumsRootDirectory, userId.ToString());
    }

    private string GetMetadataPath(int userId)
    {
        return Path.Combine(GetUserDirectory(userId), "album.json");
    }

    private static string NormalizeCategory(string value)
    {
        return string.Equals(value, "Family", StringComparison.OrdinalIgnoreCase)
            ? "Family"
            : "Personal";
    }
}
