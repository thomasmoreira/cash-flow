using CashFlow.Domain.Entities;

namespace CashFlow.Application.Contracts;

public interface ITransactionService
{
    Task<(IEnumerable<Transaction> Items, int TotalItems)> GetTransactionsPaginatedAsync(int page, int pageSize);
}
