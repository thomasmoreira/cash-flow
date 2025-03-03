using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;
using CashFlow.Domain.Services;

namespace CashFlow.Consolidating.Services;

public class ConsolidatingService : IConsolidatingService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IConsolidatingRepository _consolidatingRepository;
    private ILogger<ConsolidatingService> _logger;

    public ConsolidatingService(ITransactionRepository transactionRepository, IConsolidatingRepository consolidatingRepository, ILogger<ConsolidatingService> logger)
    {
        _transactionRepository = transactionRepository;
        _consolidatingRepository = consolidatingRepository;
        _logger = logger;
    }

    public async Task<bool> Consolidate(Guid TransactionId)
    {
        _logger.LogInformation("Consolidating transaction {TransactionId}", TransactionId);

        var transaction = await _transactionRepository.GetAsync(TransactionId);

        if (transaction is not null)
        {
            _logger.LogInformation("Transaction {TransactionId} found", TransactionId);

            var amountToConsolidate = transaction.Type == Domain.Enums.TransactionType.Expense ? transaction.Amount * -1 : transaction.Amount;

            var dailyConsolidation = await _consolidatingRepository.GetDailyConsolidatingAsync(transaction.Date);

            var newAmount = dailyConsolidation is not null ? dailyConsolidation.Amount + amountToConsolidate : amountToConsolidate;

            dailyConsolidation = new DailyConsolidation(transaction.Date, newAmount);

            _logger.LogInformation("Daily consolidation for {Date} is {Amount}", transaction.Date, dailyConsolidation.Amount);  

            if (dailyConsolidation is not null)
            {
                await _consolidatingRepository.UpdateAsync(dailyConsolidation);
                _logger.LogInformation("Daily consolidation for {Date} updated", transaction.Date);
            }
            else
            {
                await _consolidatingRepository.AddAsync(dailyConsolidation);
                _logger.LogInformation("Daily consolidation for {Date} added", transaction.Date);
            }

            return true;
        }

        return false;
    }

    public async Task<decimal> DailyConsolidateAsync()
    {
        return 0;
    }
}
