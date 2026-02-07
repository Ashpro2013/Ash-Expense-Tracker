using ExpenseIncomeTracker.Domain.Enums;

namespace ExpenseIncomeTracker.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string UserId { get; set; } = string.Empty;

    public List<Transaction> Transactions { get; set; } = new();
}
