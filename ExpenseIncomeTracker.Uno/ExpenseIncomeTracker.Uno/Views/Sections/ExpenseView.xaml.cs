namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class ExpenseView : UserControl
{
    public event RoutedEventHandler? AddExpenseRequested;
    public event RoutedEventHandler? EditExpenseRequested;
    public event RoutedEventHandler? DeleteExpenseRequested;

    public ExpenseView()
    {
        InitializeComponent();
    }

    private void OnAddExpenseClicked(object sender, RoutedEventArgs e) => AddExpenseRequested?.Invoke(sender, e);
    private void OnEditExpenseClicked(object sender, RoutedEventArgs e) => EditExpenseRequested?.Invoke(sender, e);
    private void OnDeleteExpenseClicked(object sender, RoutedEventArgs e) => DeleteExpenseRequested?.Invoke(sender, e);
}
