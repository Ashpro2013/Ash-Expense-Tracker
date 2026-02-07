using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;

namespace ExpenseIncomeTracker.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetByTypeAsync(string userId, CategoryType type, CancellationToken cancellationToken = default);
    Task<Category?> GetAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
