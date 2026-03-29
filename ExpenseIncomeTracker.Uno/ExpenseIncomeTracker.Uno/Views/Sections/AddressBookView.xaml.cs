namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class AddressBookView : UserControl
{
    public event RoutedEventHandler? AddAddressRequested;
    public event RoutedEventHandler? EditAddressRequested;
    public event RoutedEventHandler? DeleteAddressRequested;

    public AddressBookView()
    {
        InitializeComponent();
    }

    private void OnAddAddressClicked(object sender, RoutedEventArgs e) => AddAddressRequested?.Invoke(sender, e);
    private void OnEditAddressClicked(object sender, RoutedEventArgs e) => EditAddressRequested?.Invoke(sender, e);
    private void OnDeleteAddressClicked(object sender, RoutedEventArgs e) => DeleteAddressRequested?.Invoke(sender, e);
}
