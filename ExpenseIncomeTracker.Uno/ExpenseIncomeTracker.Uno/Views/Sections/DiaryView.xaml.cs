namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class DiaryView : UserControl
{
    public event RoutedEventHandler? AddDiaryRequested;
    public event RoutedEventHandler? EditDiaryRequested;
    public event RoutedEventHandler? DeleteDiaryRequested;

    public DiaryView()
    {
        InitializeComponent();
    }

    private void OnAddDiaryClicked(object sender, RoutedEventArgs e) => AddDiaryRequested?.Invoke(sender, e);
    private void OnEditDiaryClicked(object sender, RoutedEventArgs e) => EditDiaryRequested?.Invoke(sender, e);
    private void OnDeleteDiaryClicked(object sender, RoutedEventArgs e) => DeleteDiaryRequested?.Invoke(sender, e);
}
