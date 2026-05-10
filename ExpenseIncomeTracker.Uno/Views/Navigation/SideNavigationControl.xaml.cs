using System.ComponentModel;
using ExpenseIncomeTracker.Uno.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ExpenseIncomeTracker.Uno.Views.Navigation;

public sealed partial class SideNavigationControl : UserControl
{
    private MainViewModel? _subscribedViewModel;

    public SideNavigationControl()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AttachViewModel();
        SyncSelectionFromViewModel();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachViewModel();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        AttachViewModel();
        SyncSelectionFromViewModel();
    }

    private void AttachViewModel()
    {
        var currentViewModel = ViewModel;
        if (ReferenceEquals(_subscribedViewModel, currentViewModel))
        {
            return;
        }

        DetachViewModel();
        _subscribedViewModel = currentViewModel;

        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void DetachViewModel()
    {
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedViewModel = null;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedSection))
        {
            SyncSelectionFromViewModel();
        }
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section)
        {
            ViewModel?.SelectSectionCommand.Execute(section);

            if (ViewModel?.IsSidebarOpen == true)
            {
                ViewModel.IsSidebarOpen = false;
            }
        }
    }

    private void SyncSelectionFromViewModel()
    {
        var vm = ViewModel;
        if (vm is null)
        {
            return;
        }

        foreach (var item in RootNav.MenuItems.OfType<NavigationViewItem>())
        {
            if (string.Equals(item.Tag?.ToString(), vm.SelectedSection, StringComparison.OrdinalIgnoreCase))
            {
                if (!ReferenceEquals(RootNav.SelectedItem, item))
                {
                    RootNav.SelectedItem = item;
                }

                return;
            }
        }
    }
}
