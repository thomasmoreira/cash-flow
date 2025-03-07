using CashFlow.Application.Dtos;
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

    public async Task<Transaction?> GetAsync(Guid Id)
    {
        return await _context.Transactions
                .Where(t => t.Id == Id)
                .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<Transaction> Items, int TotalItems)> GetTransactionsPaginatedAsync(int page, int pageSize)
    {
        var query = _context.Transactions.AsQueryable();
        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
