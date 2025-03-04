using CashFlow.Domain.Entities;

namespace CashFlow.Application.Contracts;

public interface IConsolidationService
{

    Task Consolidate(Guid TransactionId);    
    Task<DailyConsolidation?> DailyConsolidationAsync(DateTime date);
    Task<IEnumerable<DailyConsolidation>?> ConsolidateBalanceReportAsync();

}
