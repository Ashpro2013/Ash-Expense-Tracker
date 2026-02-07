using ExpenseIncomeTracker.Application.Interfaces;
using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;
using ExpenseIncomeTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetByTypeAsync(string userId, CategoryType type, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .Where(c => c.UserId == userId && c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
        if (category is null)
        {
            return;
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
