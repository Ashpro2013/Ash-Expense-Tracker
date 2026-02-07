using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;
using ExpenseIncomeTracker.Infrastructure.Services;
using ExpenseIncomeTracker.Tests.Helpers;
using Xunit;

namespace ExpenseIncomeTracker.Tests.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateAndGetAll_ReturnsTransactionsForUser()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new TransactionService(db);
        var userId = "user-1";

        var incomeCategory = new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId };
        var expenseCategory = new Category { Name = "Rent", Type = CategoryType.Expense, UserId = userId };
        db.Categories.AddRange(incomeCategory, expenseCategory);
        await db.SaveChangesAsync();

        await service.CreateAsync(new Transaction
        {
            Amount = 5000,
            CategoryId = incomeCategory.Id,
            TransactionDate = new DateTime(2026, 1, 2),
            UserId = userId
        });
        await service.CreateAsync(new Transaction
        {
            Amount = 1500,
            CategoryId = expenseCategory.Id,
            TransactionDate = new DateTime(2026, 1, 3),
            UserId = userId
        });

        var transactions = await service.GetAllAsync(userId);
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task GetAll_IgnoresOtherUsers()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new TransactionService(db);

        var categoryUser1 = new Category { Name = "Salary", Type = CategoryType.Income, UserId = "user-1" };
        var categoryUser2 = new Category { Name = "Salary", Type = CategoryType.Income, UserId = "user-2" };
        db.Categories.AddRange(categoryUser1, categoryUser2);
        await db.SaveChangesAsync();

        await service.CreateAsync(new Transaction
        {
            Amount = 5000,
            CategoryId = categoryUser1.Id,
            TransactionDate = new DateTime(2026, 1, 2),
            UserId = "user-1"
        });
        await service.CreateAsync(new Transaction
        {
            Amount = 6000,
            CategoryId = categoryUser2.Id,
            TransactionDate = new DateTime(2026, 1, 3),
            UserId = "user-2"
        });

        var transactions = await service.GetAllAsync("user-1");
        Assert.Single(transactions);
        Assert.Equal(5000, transactions[0].Amount);
    }

    [Fact]
    public async Task GetByType_FiltersByCategoryType()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new TransactionService(db);
        var userId = "user-1";

        var incomeCategory = new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId };
        var expenseCategory = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.AddRange(incomeCategory, expenseCategory);
        await db.SaveChangesAsync();

        await service.CreateAsync(new Transaction
        {
            Amount = 2000,
            CategoryId = incomeCategory.Id,
            TransactionDate = new DateTime(2026, 1, 2),
            UserId = userId
        });
        await service.CreateAsync(new Transaction
        {
            Amount = 200,
            CategoryId = expenseCategory.Id,
            TransactionDate = new DateTime(2026, 1, 3),
            UserId = userId
        });

        var income = await service.GetByTypeAsync(userId, CategoryType.Income);
        var expense = await service.GetByTypeAsync(userId, CategoryType.Expense);

        Assert.Single(income);
        Assert.Single(expense);
        Assert.Equal(2000, income[0].Amount);
        Assert.Equal(200, expense[0].Amount);
    }

    [Fact]
    public async Task UpdateAsync_ChangesAmount()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new TransactionService(db);
        var userId = "user-1";

        var category = new Category { Name = "Side Hustle", Type = CategoryType.Income, UserId = userId };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var transaction = await service.CreateAsync(new Transaction
        {
            Amount = 100,
            CategoryId = category.Id,
            TransactionDate = new DateTime(2026, 1, 4),
            UserId = userId
        });

        transaction.Amount = 250;
        await service.UpdateAsync(transaction);

        var updated = await service.GetAsync(transaction.Id, userId);
        Assert.NotNull(updated);
        Assert.Equal(250, updated!.Amount);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTransaction()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new TransactionService(db);
        var userId = "user-1";

        var category = new Category { Name = "Utilities", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var transaction = await service.CreateAsync(new Transaction
        {
            Amount = 90,
            CategoryId = category.Id,
            TransactionDate = new DateTime(2026, 1, 5),
            UserId = userId
        });

        await service.DeleteAsync(transaction.Id, userId);
        var deleted = await service.GetAsync(transaction.Id, userId);

        Assert.Null(deleted);
    }
}
