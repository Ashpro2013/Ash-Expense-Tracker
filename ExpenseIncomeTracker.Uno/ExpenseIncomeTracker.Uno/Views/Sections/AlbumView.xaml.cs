namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class AlbumView : UserControl
{
    public event RoutedEventHandler? PickFromGalleryRequested;
    public event RoutedEventHandler? TakePhotoRequested;
    public event RoutedEventHandler? DeleteAlbumImageRequested;
    public event RoutedEventHandler? ShowPreviousAlbumImageRequested;
    public event RoutedEventHandler? ShowNextAlbumImageRequested;
    public event SelectionChangedEventHandler? AlbumFlipSelectionChanged;

    public AlbumView()
    {
        InitializeComponent();
    }

    private void OnPickFromGalleryClicked(object sender, RoutedEventArgs e) => PickFromGalleryRequested?.Invoke(sender, e);
    private void OnTakePhotoClicked(object sender, RoutedEventArgs e) => TakePhotoRequested?.Invoke(sender, e);
    private void OnDeleteAlbumImageClicked(object sender, RoutedEventArgs e) => DeleteAlbumImageRequested?.Invoke(sender, e);
    private void OnShowPreviousAlbumImageClicked(object sender, RoutedEventArgs e) => ShowPreviousAlbumImageRequested?.Invoke(sender, e);
    private void OnShowNextAlbumImageClicked(object sender, RoutedEventArgs e) => ShowNextAlbumImageRequested?.Invoke(sender, e);
    private void OnAlbumFlipViewSelectionChanged(object sender, SelectionChangedEventArgs e) => AlbumFlipSelectionChanged?.Invoke(sender, e);
}
