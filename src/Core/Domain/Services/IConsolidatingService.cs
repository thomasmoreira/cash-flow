using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Services;

public interface IConsolidatingService
{

    Task<bool> Consolidate(Guid TransactionId);    
    Task<DailyConsolidation?> DailyConsolidationAsync(DateTime date);
    Task<IEnumerable<DailyConsolidation>?> ConsolidateBalanceReportAsync();

}
