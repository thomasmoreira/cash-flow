using CashFlow.Application.Contracts;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CashFlow.Infraestructure.Services;

public class ConsolidatingService : IConsolidationService
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

    public async Task Consolidate(Guid TransactionId)
    {
        _logger.LogInformation("Consolidating transaction {TransactionId}", TransactionId);

        var transaction = await _transactionRepository.GetAsync(TransactionId);

        if (transaction is not null)
        {
            _logger.LogInformation("Transaction {TransactionId} found", TransactionId);

            var amountToConsolidate = transaction.Type == Domain.Enums.TransactionType.Debit ? transaction.Amount * -1 : transaction.Amount;

            var dailyConsolidationData = await _consolidatingRepository.GetDailyConsolidatingAsync(transaction.Date);

            var newAmount = dailyConsolidationData is not null ? dailyConsolidationData.Amount + amountToConsolidate : amountToConsolidate;
           
            _logger.LogInformation("Daily consolidation for {Date} is {Amount}", transaction.Date, newAmount);  

            if (dailyConsolidationData is not null)
            {
                dailyConsolidationData.Amount = newAmount;
                await _consolidatingRepository.UpdateAsync(dailyConsolidationData);
                _logger.LogInformation("Daily consolidation for {Date} updated", transaction.Date);
            }
            else
            {
                var dailyConsolidation = new DailyConsolidation(transaction.Date.Date, newAmount);
                await _consolidatingRepository.AddAsync(dailyConsolidation);
                _logger.LogInformation("Daily consolidation for {Date} added", transaction.Date);
            }

            //return true;
        }

        //return false;
    }

    public async Task<DailyConsolidation?> DailyConsolidationAsync(DateTime date)
    {
        return await _consolidatingRepository.GetDailyConsolidatingAsync(date);
    }

    public async Task<IEnumerable<DailyConsolidation>?> ConsolidateBalanceReportAsync()
    {
        return await _consolidatingRepository.GetConsolidateBalanceReport();
    }
}
