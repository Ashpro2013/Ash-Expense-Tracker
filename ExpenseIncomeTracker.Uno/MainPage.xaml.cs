using System.ComponentModel;
using ExpenseIncomeTracker.Uno.Services;
using ExpenseIncomeTracker.Uno.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ExpenseIncomeTracker.Uno;

public sealed partial class MainPage : Page
{
    private bool _isInitialized;

    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();

        ViewModel = new MainViewModel(new AppStateService(new LocalStoreService()), new ActivationService());
        DataContext = ViewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        await ViewModel.InitializeCommand.ExecuteAsync(null);
        SyncPasswordBoxes();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnAuthPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && ViewModel.AuthPassword != passwordBox.Password)
        {
            ViewModel.AuthPassword = passwordBox.Password;
        }
    }

    private void OnAuthConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && ViewModel.AuthConfirmPassword != passwordBox.Password)
        {
            ViewModel.AuthConfirmPassword = passwordBox.Password;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.AuthPassword) or nameof(MainViewModel.AuthConfirmPassword))
        {
            SyncPasswordBoxes();
        }
    }

    private void SyncPasswordBoxes()
    {
        if (AuthPasswordBox.Password != ViewModel.AuthPassword)
        {
            AuthPasswordBox.Password = ViewModel.AuthPassword;
        }

        if (AuthConfirmPasswordBox.Password != ViewModel.AuthConfirmPassword)
        {
            AuthConfirmPasswordBox.Password = ViewModel.AuthConfirmPassword;
        }
    }
}
