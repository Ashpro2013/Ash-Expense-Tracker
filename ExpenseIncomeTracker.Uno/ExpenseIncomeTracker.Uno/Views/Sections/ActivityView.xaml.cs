namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class ActivityView : UserControl
{
    public event RoutedEventHandler? AddActivityRequested;
    public event RoutedEventHandler? EditActivityRequested;
    public event RoutedEventHandler? ToggleActivityDoneRequested;
    public event RoutedEventHandler? DeleteActivityRequested;

    public ActivityView()
    {
        InitializeComponent();
    }

    private void OnAddActivityClicked(object sender, RoutedEventArgs e) => AddActivityRequested?.Invoke(sender, e);
    private void OnEditActivityClicked(object sender, RoutedEventArgs e) => EditActivityRequested?.Invoke(sender, e);
    private void OnToggleActivityDoneClicked(object sender, RoutedEventArgs e) => ToggleActivityDoneRequested?.Invoke(sender, e);
    private void OnDeleteActivityClicked(object sender, RoutedEventArgs e) => DeleteActivityRequested?.Invoke(sender, e);
}
