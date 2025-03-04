using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction lancamento);
    Task<Transaction?> GetAsync(Guid Id);

    Task<(IEnumerable<Transaction> Items, int TotalItems)> GetTransactionsPaginatedAsync(int page, int pageSize);
}
