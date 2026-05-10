using ExpenseIncomeTracker.Uno.Interfaces;
using System.Text.Json;
using ExpenseIncomeTracker.Uno.Models;
using Windows.Storage;

namespace ExpenseIncomeTracker.Uno.Services;

public sealed class LocalStoreService : ILocalStoreService
{
    private const string FileName = "expense_tracker_state.json";

    public async Task<AppState> LoadAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(FileName) as StorageFile;
            if (file is null)
            {
                return new AppState();
            }

            var json = await FileIO.ReadTextAsync(file);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppState();
            }

            return JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
        }
        catch
        {
            return new AppState();
        }
    }

    public async Task SaveAsync(AppState state)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await FileIO.WriteTextAsync(file, json);
    }
}
