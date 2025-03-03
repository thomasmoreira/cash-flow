using CashFlow.Application.Commands;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.Repositories;
using MassTransit;
using MediatR;

namespace CashFlow.Application.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private ITransactionRepository _repository;
    private readonly IBus _bus;

    public CreateTransactionCommandHandler(ITransactionRepository repository, IBus bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var type = (TransactionType)request.Type;
        var transaction = new Transaction(Guid.NewGuid(), request.Date, type, request.Amount, request.Description);

        await _repository.AddAsync(transaction);

        var @event = new TransactionCreatedEvent
        {
            TransactionId = transaction.Id
        };

        await _bus.Publish(@event, cancellationToken);

        return transaction.Id;
    }
}
