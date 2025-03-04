using CashFlow.Domain.Enums;

namespace CashFlow.Application.Dtos;

public class TransactionDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}
