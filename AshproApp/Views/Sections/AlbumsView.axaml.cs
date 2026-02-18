using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AshproApp.ViewModels;

namespace AshproApp.Views.Sections;

public partial class AlbumsView : UserControl
{
    private MainWindowViewModel? _viewerVm;
    private bool _isPanning;
    private Point _panStartPoint;
    private Vector _panStartOffset;
    private DateTime _lastTapAtUtc = DateTime.MinValue;
    private Point _lastTapPosition;

    public AlbumsView()
    {
        InitializeComponent();
        DataContextChanged += AlbumsView_DataContextChanged;
    }

    private async void UploadImages_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose images",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp" }
                }
            }
        });

        var paths = files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();

        if (paths.Count == 0)
        {
            return;
        }

        await vm.UploadAlbumFilesAsync(paths);
    }

    private void AlbumsView_DataContextChanged(object? sender, EventArgs e)
    {
        if (_viewerVm is not null)
        {
            _viewerVm.PropertyChanged -= ViewerVm_PropertyChanged;
        }

        _viewerVm = DataContext as MainWindowViewModel;

        if (_viewerVm is not null)
        {
            _viewerVm.PropertyChanged += ViewerVm_PropertyChanged;
            UpdateViewerViewport();
        }
    }

    private void ViewerVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewerVm is null)
        {
            return;
        }

        if (e.PropertyName is nameof(MainWindowViewModel.IsAlbumViewerOpen) or nameof(MainWindowViewModel.AlbumViewerImage))
        {
            if (!_viewerVm.IsAlbumViewerOpen)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                ViewerScroll.Offset = new Vector(0, 0);
                UpdateViewerViewport();
            }, DispatcherPriority.Background);
        }
    }

    private void ViewerViewport_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateViewerViewport();
    }

    private void UpdateViewerViewport()
    {
        if (_viewerVm is null)
        {
            return;
        }

        var width = Math.Max(0, ViewerViewport.Bounds.Width - 2);
        var height = Math.Max(0, ViewerViewport.Bounds.Height - 2);
        _viewerVm.SetAlbumViewerViewport(width, height);
    }

    private void ViewerScroll_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_viewerVm is null || !_viewerVm.IsAlbumViewerOpen)
        {
            return;
        }

        var factor = e.Delta.Y > 0 ? 1.15 : 1 / 1.15;
        ApplyZoom(_viewerVm.AlbumViewerZoom * factor, e.GetPosition(ViewerScroll));
        e.Handled = true;
    }

    private void ViewerImage_Tapped(object? sender, TappedEventArgs e)
    {
        if (_viewerVm is null || !_viewerVm.IsAlbumViewerOpen)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var tapPosition = e.GetPosition(ViewerScroll);
        var elapsed = nowUtc - _lastTapAtUtc;
        var moved = Math.Abs(tapPosition.X - _lastTapPosition.X) + Math.Abs(tapPosition.Y - _lastTapPosition.Y);
        var isDoubleTap = elapsed.TotalMilliseconds <= 350 && moved <= 24;

        _lastTapAtUtc = nowUtc;
        _lastTapPosition = tapPosition;

        if (!isDoubleTap)
        {
            return;
        }

        var targetZoom = _viewerVm.AlbumViewerZoom > 1.05 ? 1.0 : 2.5;
        ApplyZoom(targetZoom, tapPosition);
        e.Handled = true;
    }

    private void ApplyZoom(double targetZoom, Point anchor)
    {
        if (_viewerVm is null)
        {
            return;
        }

        var currentZoom = Math.Max(0.01, _viewerVm.AlbumViewerZoom);
        var clampedTargetZoom = Math.Clamp(targetZoom, 1.0, 6.0);
        if (Math.Abs(clampedTargetZoom - currentZoom) < 0.001)
        {
            return;
        }

        var oldOffset = ViewerScroll.Offset;
        var contentX = (oldOffset.X + anchor.X) / currentZoom;
        var contentY = (oldOffset.Y + anchor.Y) / currentZoom;

        _viewerVm.AlbumViewerZoom = clampedTargetZoom;

        Dispatcher.UIThread.Post(() =>
        {
            var rawOffsetX = contentX * clampedTargetZoom - anchor.X;
            var rawOffsetY = contentY * clampedTargetZoom - anchor.Y;
            ViewerScroll.Offset = new Vector(
                ClampOffset(rawOffsetX, ViewerScroll.Extent.Width, ViewerScroll.Viewport.Width),
                ClampOffset(rawOffsetY, ViewerScroll.Extent.Height, ViewerScroll.Viewport.Height));
        }, DispatcherPriority.Background);
    }

    private void ViewerScroll_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewerVm is null || !_viewerVm.IsAlbumViewerOpen)
        {
            return;
        }

        var point = e.GetCurrentPoint(ViewerScroll);
        if (e.Pointer.Type != PointerType.Touch && !point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isPanning = true;
        _panStartPoint = e.GetPosition(ViewerScroll);
        _panStartOffset = ViewerScroll.Offset;
        e.Pointer.Capture(ViewerScroll);
        e.Handled = true;
    }

    private void ViewerScroll_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning)
        {
            return;
        }

        var current = e.GetPosition(ViewerScroll);
        var delta = current - _panStartPoint;

        var targetX = _panStartOffset.X - delta.X;
        var targetY = _panStartOffset.Y - delta.Y;

        ViewerScroll.Offset = new Vector(
            ClampOffset(targetX, ViewerScroll.Extent.Width, ViewerScroll.Viewport.Width),
            ClampOffset(targetY, ViewerScroll.Extent.Height, ViewerScroll.Viewport.Height));

        e.Handled = true;
    }

    private void ViewerScroll_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPanning)
        {
            return;
        }

        _isPanning = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private static double ClampOffset(double value, double extent, double viewport)
    {
        var maxOffset = Math.Max(0, extent - viewport);
        return Math.Clamp(value, 0, maxOffset);
    }
}
