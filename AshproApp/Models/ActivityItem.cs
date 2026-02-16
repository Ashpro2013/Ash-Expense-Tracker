namespace AshproApp.Models;

public enum ActivityStatus
{
    Planned = 1,
    InProgress = 2,
    Done = 3
}

public sealed class ActivityItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsImportant { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
