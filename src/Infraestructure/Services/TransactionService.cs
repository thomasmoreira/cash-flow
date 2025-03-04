using CashFlow.Application.Contracts;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;

namespace CashFlow.Infraestructure.Services;

public class TransactionService : ITransactionService
{
    private ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public Task<(IEnumerable<Transaction> Items, int TotalItems)> GetTransactionsPaginatedAsync(int page, int pageSize)
    {
        return _transactionRepository.GetTransactionsPaginatedAsync(page, pageSize);
    }
}
