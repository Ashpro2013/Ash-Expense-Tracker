using System.ComponentModel.DataAnnotations;

namespace ExpenseIncomeTracker.Web.Models;

public sealed class DiaryEntry
{
    [Key]
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; } = DateTime.Today;

    public List<string> Tags { get; set; } = new();

    public int Mood { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
