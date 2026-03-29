namespace ExpenseIncomeTracker.Uno.Models;

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
