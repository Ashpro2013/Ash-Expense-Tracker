namespace ExpenseIncomeTracker.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public Category? Category { get; set; }
}
