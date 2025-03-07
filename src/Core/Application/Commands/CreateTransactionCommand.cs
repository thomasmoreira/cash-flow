using MediatR;

namespace CashFlow.Application.Commands;

public class CreateTransactionCommand : IRequest<Guid>
{
    public DateTime Date { get; set; }
    public int Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}
