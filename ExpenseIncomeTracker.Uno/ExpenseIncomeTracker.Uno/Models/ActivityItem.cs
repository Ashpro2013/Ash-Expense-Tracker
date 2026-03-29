namespace ExpenseIncomeTracker.Uno.Models;

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
