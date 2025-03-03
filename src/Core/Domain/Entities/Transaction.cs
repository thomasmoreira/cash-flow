using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Entities;

public class Transaction
{
    public Transaction(Guid id, DateTime date, TransactionType type, decimal amount, string description)
    {
        Id = id;
        Date = date;
        Type = type;
        Amount = amount;
        Description = description;
    }

    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
}
