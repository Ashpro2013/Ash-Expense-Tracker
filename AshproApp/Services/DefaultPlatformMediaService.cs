namespace AshproApp.Services;

public sealed class DefaultPlatformMediaService : IPlatformMediaService
{
    public bool CanCapturePhoto => false;
    public bool CanShareFiles => false;

    public Task<string?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<bool> ShareImageAsync(string filePath, string? text = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
