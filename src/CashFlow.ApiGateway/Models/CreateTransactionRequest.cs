namespace CashFlow.ApiGateway.Models;

public class CreateTransactionRequest
{
    public DateTime Date { get; set; }
    public int Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}
