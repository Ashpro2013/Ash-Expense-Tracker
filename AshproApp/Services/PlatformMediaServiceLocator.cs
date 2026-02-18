namespace AshproApp.Services;

public static class PlatformMediaServiceLocator
{
    private static IPlatformMediaService _current = new DefaultPlatformMediaService();

    public static IPlatformMediaService Current
    {
        get => _current;
        set => _current = value ?? new DefaultPlatformMediaService();
    }

    public static void Reset()
    {
        _current = new DefaultPlatformMediaService();
    }
}
