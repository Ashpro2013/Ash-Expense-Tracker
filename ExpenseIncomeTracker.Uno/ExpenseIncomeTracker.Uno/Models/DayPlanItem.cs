namespace ExpenseIncomeTracker.Uno.Models;

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
