namespace CashFlow.Domain.Entities;

public class DailyConsolidation
{
    public DailyConsolidation(DateTime date, decimal amount)
    {
        Date = date;
        Amount = amount;
    }

    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
