namespace AshproApp.Models;

public sealed class DiaryEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string TagsCsv { get; set; } = string.Empty;
    public int Mood { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
