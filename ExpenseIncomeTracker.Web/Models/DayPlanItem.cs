using System.ComponentModel.DataAnnotations;

namespace ExpenseIncomeTracker.Web.Models;

public sealed class DayPlanItem
{
    [Key]
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime PlanDate { get; set; } = DateTime.Today;

    public string Title { get; set; } = string.Empty;

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
