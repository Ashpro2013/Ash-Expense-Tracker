using ExpenseIncomeTracker.Uno.Interfaces;
using ExpenseIncomeTracker.Uno.Models;

namespace ExpenseIncomeTracker.Uno.Services;

public sealed class AppStateService : IAppStateService
{
    private readonly ILocalStoreService _storeService;

    public AppStateService(ILocalStoreService storeService)
    {
        _storeService = storeService;
    }

    public Task<AppState> LoadAsync()
    {
        return _storeService.LoadAsync();
    }

    public Task SaveAsync(AppState state)
    {
        return _storeService.SaveAsync(state);
    }
}
