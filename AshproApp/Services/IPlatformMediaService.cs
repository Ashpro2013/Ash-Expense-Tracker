namespace AshproApp.Services;

public interface IPlatformMediaService
{
    bool CanCapturePhoto { get; }
    bool CanShareFiles { get; }

    Task<string?> CapturePhotoAsync(CancellationToken cancellationToken = default);
    Task<bool> ShareImageAsync(string filePath, string? text = null, CancellationToken cancellationToken = default);
}
