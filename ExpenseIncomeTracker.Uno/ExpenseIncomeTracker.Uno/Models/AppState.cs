namespace ExpenseIncomeTracker.Uno.Models;

public sealed class AppState
{
    public List<UserAccount> Users { get; set; } = new();
    public string? CurrentUserEmail { get; set; }
    public List<FinanceEntry> FinanceEntries { get; set; } = new();
    public List<ActivityItem> ActivityItems { get; set; } = new();
    public List<DiaryEntry> DiaryEntries { get; set; } = new();
    public List<DayPlanItem> DayPlanItems { get; set; } = new();
    public List<AlbumImageItem> AlbumImages { get; set; } = new();
    public List<AddressBookEntry> AddressBookEntries { get; set; } = new();
    public List<PasswordDirectoryEntry> PasswordDirectoryEntries { get; set; } = new();
}
