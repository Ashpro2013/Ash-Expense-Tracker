using ExpenseIncomeTracker.Application.Interfaces;
using ExpenseIncomeTracker.Domain.Entities;
using ExpenseIncomeTracker.Domain.Enums;
using ExpenseIncomeTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseIncomeTracker.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Transaction>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByTypeAsync(string userId, CategoryType type, CancellationToken cancellationToken = default)
    {
        return await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Category != null && t.Category.Type == type)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        if (transaction is null)
        {
            return;
        }

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
