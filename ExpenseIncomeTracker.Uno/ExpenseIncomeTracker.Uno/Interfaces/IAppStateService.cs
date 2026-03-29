using ExpenseIncomeTracker.Uno.Models;

namespace ExpenseIncomeTracker.Uno.Interfaces;

public interface IAppStateService
{
    Task<AppState> LoadAsync();
    Task SaveAsync(AppState state);
}
