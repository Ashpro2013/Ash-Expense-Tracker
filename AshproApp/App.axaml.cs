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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var dbContextFactory = new AppDbContextFactory();
            var authService = new AuthService(dbContextFactory);
            var rememberMeService = new RememberMeService();
            var albumService = new AlbumService();
            var financeEntryService = new FinanceEntryService(dbContextFactory);
            var diaryEntryService = new DiaryEntryService(dbContextFactory);
            var activityItemService = new ActivityItemService(dbContextFactory);
            var viewModel = new MainWindowViewModel(
                authService,
                rememberMeService,
                albumService,
                financeEntryService,
                diaryEntryService,
                activityItemService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
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
