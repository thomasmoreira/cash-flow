using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories;

public interface IConsolidatingRepository
{
    Task<DailyConsolidation?> GetDailyConsolidatingAsync(DateTime date);
    Task<IEnumerable<DailyConsolidation>?> GetConsolidateBalanceReport();
    Task AddAsync(DailyConsolidation dailyConsolidation);
    Task UpdateAsync(DailyConsolidation dailyConsolidation);
}
