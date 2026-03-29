using ExpenseIncomeTracker.Uno.Models;

namespace ExpenseIncomeTracker.Uno.Interfaces;

public interface ILocalStoreService
{
    Task<AppState> LoadAsync();
    Task SaveAsync(AppState state);
}
