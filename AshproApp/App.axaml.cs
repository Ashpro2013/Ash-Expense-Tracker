using System.Linq;
using AshproApp.Data;
using AshproApp.Services;
using AshproApp.ViewModels;
using AshproApp.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

namespace AshproApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        var viewModel = CreateMainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = viewModel
            };

            _ = viewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MainWindowViewModel CreateMainWindowViewModel()
    {
        var dbContextFactory = new AppDbContextFactory();
        var authService = new AuthService(dbContextFactory);
        var rememberMeService = new RememberMeService();
        var albumService = new AlbumService();
        var platformMediaService = PlatformMediaServiceLocator.Current;
        var financeEntryService = new FinanceEntryService(dbContextFactory);
        var diaryEntryService = new DiaryEntryService(dbContextFactory);
        var activityItemService = new ActivityItemService(dbContextFactory);

        return new MainWindowViewModel(
            authService,
            rememberMeService,
            albumService,
            platformMediaService,
            financeEntryService,
            diaryEntryService,
            activityItemService);
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var plugins = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        foreach (var plugin in plugins)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
