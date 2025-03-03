namespace CashFlow.Domain.Services;

public interface IConsolidatingService
{
    Task<decimal> DailyConsolidateAsync();
}
