using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infraestructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Transaction>> GetAsync(DateTime data)
    {
        return await _context.Transactions
                .Where(t => t.Date.Date == data.Date)
                .ToListAsync();
    }
}
