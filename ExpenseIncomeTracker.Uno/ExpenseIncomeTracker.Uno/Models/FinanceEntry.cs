namespace ExpenseIncomeTracker.Uno.Models;

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
