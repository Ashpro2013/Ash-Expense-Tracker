namespace AshproApp.Models;

public enum FinanceEntryType
{
    Income = 1,
    Expense = 2
}

public sealed class FinanceEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string? Note { get; set; }
    public FinanceEntryType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
