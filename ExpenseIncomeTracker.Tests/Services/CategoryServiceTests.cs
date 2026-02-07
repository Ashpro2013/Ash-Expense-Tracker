using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;
using ExpenseIncomeTracker.Infrastructure.Services;
using ExpenseIncomeTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseIncomeTracker.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAndFetchByType_ReturnsOnlyMatchingCategories()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new CategoryService(db);
        var userId = "user-1";

        await service.CreateAsync(new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId });
        await service.CreateAsync(new Category { Name = "Rent", Type = CategoryType.Expense, UserId = userId });

        var income = await service.GetByTypeAsync(userId, CategoryType.Income);
        var expense = await service.GetByTypeAsync(userId, CategoryType.Expense);

        Assert.Single(income);
        Assert.Single(expense);
        Assert.Equal("Salary", income[0].Name);
        Assert.Equal("Rent", expense[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesName()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new CategoryService(db);
        var userId = "user-1";

        var category = await service.CreateAsync(new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId });
        category.Name = "Groceries";
        await service.UpdateAsync(category);

        var updated = await service.GetAsync(category.Id, userId);
        Assert.NotNull(updated);
        Assert.Equal("Groceries", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new CategoryService(db);
        var userId = "user-1";

        var category = await service.CreateAsync(new Category { Name = "Bills", Type = CategoryType.Expense, UserId = userId });

        await service.DeleteAsync(category.Id, userId);
        var deleted = await service.GetAsync(category.Id, userId);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DuplicateCategoryNameForUser_Throws()
    {
        await using var factory = new TestDbContextFactory();
        await using var db = factory.CreateDbContext();
        var service = new CategoryService(db);
        var userId = "user-1";

        await service.CreateAsync(new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId });

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.CreateAsync(new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId }));
    }
}
