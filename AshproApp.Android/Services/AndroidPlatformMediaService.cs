using Android.App;
using Android.Content;
using Android.Provider;
using Android.Webkit;
using AndroidX.Core.Content;
using AshproApp.Services;

namespace AshproApp.Android.Services;

public sealed class AndroidPlatformMediaService : IPlatformMediaService, IDisposable
{
    private const int CapturePhotoRequestCode = 4101;

    private readonly MainActivity _activity;
    private readonly object _sync = new();

    private TaskCompletionSource<string?>? _capturePhotoTaskSource;
    private string? _pendingPhotoPath;

    public AndroidPlatformMediaService(MainActivity activity)
    {
        _activity = activity;
        _activity.ActivityResultReceived += OnActivityResultReceived;
    }

    public bool CanCapturePhoto => true;
    public bool CanShareFiles => true;

    public Task<string?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult<string?>(null);
        }

        TaskCompletionSource<string?> taskSource;

        lock (_sync)
        {
            if (_capturePhotoTaskSource is not null)
            {
                return Task.FromResult<string?>(null);
            }

            taskSource = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _capturePhotoTaskSource = taskSource;
        }

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => CompleteCapture(null));
        }

        _activity.RunOnUiThread(() =>
        {
            try
            {
                var captureIntent = new Intent(MediaStore.ActionImageCapture);
                var packageManager = _activity.PackageManager;
                if (packageManager is null || captureIntent.ResolveActivity(packageManager) is null)
                {
                    CompleteCapture(null);
                    return;
                }

                var destinationPath = CreateCaptureDestinationPath();
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                var destinationFile = new Java.IO.File(destinationPath);
                var authority = $"{_activity.PackageName}.fileprovider";
                var imageUri = FileProvider.GetUriForFile(_activity, authority, destinationFile);

                _pendingPhotoPath = destinationPath;

                captureIntent.PutExtra(MediaStore.ExtraOutput, imageUri);
                captureIntent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                _activity.StartActivityForResult(captureIntent, CapturePhotoRequestCode);
            }
            catch
            {
                CompleteCapture(null);
            }
        });

        return taskSource.Task;
    }

    public Task<bool> ShareImageAsync(string filePath, string? text = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        var taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _activity.RunOnUiThread(() =>
        {
            try
            {
                var file = new Java.IO.File(filePath);
                var authority = $"{_activity.PackageName}.fileprovider";
                var imageUri = FileProvider.GetUriForFile(_activity, authority, file);

                var shareIntent = new Intent(Intent.ActionSend);
                shareIntent.SetType(GetImageMimeType(filePath));
                shareIntent.PutExtra(Intent.ExtraStream, imageUri);
                shareIntent.ClipData = ClipData.NewUri(_activity.ContentResolver, "album_image", imageUri);
                shareIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    shareIntent.PutExtra(Intent.ExtraText, text.Trim());
                }

                var chooser = Intent.CreateChooser(shareIntent, "Share image");
                _activity.StartActivity(chooser);

                taskSource.TrySetResult(true);
            }
            catch
            {
                taskSource.TrySetResult(false);
            }
        });

        return taskSource.Task;
    }

    public void Dispose()
    {
        _activity.ActivityResultReceived -= OnActivityResultReceived;
    }

    private void OnActivityResultReceived(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != CapturePhotoRequestCode)
        {
            return;
        }

        string? capturePath = null;
        var pendingPath = _pendingPhotoPath;

        if (resultCode == Result.Ok && !string.IsNullOrWhiteSpace(pendingPath) && File.Exists(pendingPath))
        {
            capturePath = pendingPath;
        }
        else if (!string.IsNullOrWhiteSpace(pendingPath) && File.Exists(pendingPath))
        {
            TryDeleteFile(pendingPath);
        }

        CompleteCapture(capturePath);
    }

    private void CompleteCapture(string? path)
    {
        TaskCompletionSource<string?>? taskSource;

        lock (_sync)
        {
            taskSource = _capturePhotoTaskSource;
            _capturePhotoTaskSource = null;
            _pendingPhotoPath = null;
        }

        taskSource?.TrySetResult(path);
    }

    private string CreateCaptureDestinationPath()
    {
        var baseDirectory = _activity.GetExternalFilesDir(global::Android.OS.Environment.DirectoryPictures)?.AbsolutePath
                            ?? _activity.FilesDir?.AbsolutePath
                            ?? _activity.CacheDir?.AbsolutePath
                            ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var captureDirectory = Path.Combine(baseDirectory, "captured");
        var fileName = $"ashpro_photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        return Path.Combine(captureDirectory, fileName);
    }

    private static string GetImageMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "image/jpeg";
        }

        return MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension) ?? "image/jpeg";
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Keep capture cancellation resilient and non-fatal.
        }
    }
}
