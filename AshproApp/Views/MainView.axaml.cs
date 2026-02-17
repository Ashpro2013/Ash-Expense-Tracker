using Avalonia;
using Avalonia.Controls;

namespace AshproApp.Views;

public partial class MainView : UserControl
{
    public static readonly StyledProperty<bool> IsCompactLayoutProperty =
        AvaloniaProperty.Register<MainView, bool>(nameof(IsCompactLayout));

    public static readonly StyledProperty<bool> IsWideLayoutProperty =
        AvaloniaProperty.Register<MainView, bool>(nameof(IsWideLayout), true);

    private const double CompactWidthThreshold = 900;

    public bool IsCompactLayout
    {
        get => GetValue(IsCompactLayoutProperty);
        private set => SetValue(IsCompactLayoutProperty, value);
    }

    public bool IsWideLayout
    {
        get => GetValue(IsWideLayoutProperty);
        private set => SetValue(IsWideLayoutProperty, value);
    }

    public MainView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        AttachedToVisualTree += (_, _) => UpdateLayoutMode(Bounds.Width);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateLayoutMode(e.NewSize.Width);
    }

    private void UpdateLayoutMode(double width)
    {
        var isCompact = width <= CompactWidthThreshold;
        if (isCompact == IsCompactLayout)
        {
            return;
        }

        IsCompactLayout = isCompact;
        IsWideLayout = !isCompact;
    }
}
