using System.ComponentModel.DataAnnotations;

namespace ExpenseIncomeTracker.Web.Models;

public enum ActivityStatus
{
    Planned,
    InProgress,
    Done
}

public sealed class ActivityItem
{
    [Key]
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;

    public bool IsImportant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}
