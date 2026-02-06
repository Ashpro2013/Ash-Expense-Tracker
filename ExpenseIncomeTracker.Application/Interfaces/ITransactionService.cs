using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;

namespace ExpenseIncomeTracker.Application.Interfaces;

public interface ITransactionService
{
    Task<List<Transaction>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByTypeAsync(string userId, CategoryType type, CancellationToken cancellationToken = default);
    Task<Transaction?> GetAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
