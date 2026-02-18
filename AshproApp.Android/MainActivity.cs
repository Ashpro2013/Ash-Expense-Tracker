using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AshproApp.Android.Services;
using AshproApp.Services;
using Avalonia;
using Avalonia.Android;

namespace AshproApp.Android;

[Activity(
    Label = "AshproApp",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private AndroidPlatformMediaService? _platformMediaService;

    internal event Action<int, Result, Intent?>? ActivityResultReceived;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        _platformMediaService = new AndroidPlatformMediaService(this);
        PlatformMediaServiceLocator.Current = _platformMediaService;
        base.OnCreate(savedInstanceState);
    }

    protected override void OnDestroy()
    {
        if (ReferenceEquals(PlatformMediaServiceLocator.Current, _platformMediaService))
        {
            PlatformMediaServiceLocator.Reset();
        }

        _platformMediaService?.Dispose();
        _platformMediaService = null;
        base.OnDestroy();
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        ActivityResultReceived?.Invoke(requestCode, resultCode, data);
    }
}
