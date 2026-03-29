namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class PasswordDirectoryView : UserControl
{
    public event RoutedEventHandler? AddPasswordRequested;
    public event RoutedEventHandler? EditPasswordRequested;
    public event RoutedEventHandler? DeletePasswordRequested;

    public PasswordDirectoryView()
    {
        InitializeComponent();
    }

    private void OnAddPasswordClicked(object sender, RoutedEventArgs e) => AddPasswordRequested?.Invoke(sender, e);
    private void OnEditPasswordClicked(object sender, RoutedEventArgs e) => EditPasswordRequested?.Invoke(sender, e);
    private void OnDeletePasswordClicked(object sender, RoutedEventArgs e) => DeletePasswordRequested?.Invoke(sender, e);
}
