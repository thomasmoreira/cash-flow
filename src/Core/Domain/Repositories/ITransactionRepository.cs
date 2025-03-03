using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction lancamento);
    Task<IEnumerable<Transaction>> GetAsync(DateTime data);
}
