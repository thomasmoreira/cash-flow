namespace CashFlow.Domain.Services;

public interface IConsolidatingService
{

    Task<bool> Consolidate(Guid TransactionId);    
    Task<decimal> DailyConsolidateAsync();
}
