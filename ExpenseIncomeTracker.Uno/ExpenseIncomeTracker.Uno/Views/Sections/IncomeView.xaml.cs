namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class IncomeView : UserControl
{
    public event RoutedEventHandler? AddIncomeRequested;
    public event RoutedEventHandler? EditIncomeRequested;
    public event RoutedEventHandler? DeleteIncomeRequested;

    public IncomeView()
    {
        InitializeComponent();
    }

    private void OnAddIncomeClicked(object sender, RoutedEventArgs e) => AddIncomeRequested?.Invoke(sender, e);
    private void OnEditIncomeClicked(object sender, RoutedEventArgs e) => EditIncomeRequested?.Invoke(sender, e);
    private void OnDeleteIncomeClicked(object sender, RoutedEventArgs e) => DeleteIncomeRequested?.Invoke(sender, e);
}
