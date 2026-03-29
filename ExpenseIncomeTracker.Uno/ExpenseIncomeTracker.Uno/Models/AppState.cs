namespace ExpenseIncomeTracker.Uno.Models;

public enum FinanceType
{
    Income,
    Expense
}

public enum ActivityStatus
{
    Planned,
    InProgress,
    Done
}

public sealed class FinanceEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public FinanceType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Today;
}

public sealed class ActivityItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; } = DateTime.Today;
    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;
    public bool IsImportant { get; set; }
}

public sealed class DiaryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string Tags { get; set; } = string.Empty;
    public int Mood { get; set; }
}

public sealed class DayPlanItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public DateTime PlanDate { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public string StartTime { get; set; } = "09:00";
    public string EndTime { get; set; } = "10:00";
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed class AlbumImageItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string SourceType { get; set; } = "Gallery";
    public DateTime AddedOn { get; set; } = DateTime.Now;
    public string ImageUri => $"ms-appdata:///local/album/{Uri.EscapeDataString(FileName)}";
}

public sealed class AppState
{
    public List<UserAccount> Users { get; set; } = new();
    public string? CurrentUserEmail { get; set; }
    public List<FinanceEntry> FinanceEntries { get; set; } = new();
    public List<ActivityItem> ActivityItems { get; set; } = new();
    public List<DiaryEntry> DiaryEntries { get; set; } = new();
    public List<DayPlanItem> DayPlanItems { get; set; } = new();
    public List<AlbumImageItem> AlbumImages { get; set; } = new();
}

public sealed class UserAccount
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
