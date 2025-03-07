using CacheFlow.Shared.Extensions;
using CashFlow.Application.Commands;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.Repositories;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private ITransactionRepository _repository;
    private readonly IBus _bus;
    private ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(ITransactionRepository repository, IBus bus, ILogger<CreateTransactionCommandHandler> logger)
    {
        _repository = repository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        
        _logger.LogInformation("Creating transaction");

        var type = (TransactionType)request.Type;
        var transaction = new Transaction(Guid.NewGuid(), request.Date, type, request.Amount, request.Description);

        await _repository.AddAsync(transaction);

        _logger.LogInformation("Transaction {TransactionId} created", transaction.Id);

        _logger.LogInformation("Publishing TransactionCreatedEvent");

        var @event = new TransactionCreatedEvent
        {
            TransactionId = transaction.Id
        };

        await _bus.Publish(@event, cancellationToken);

        _logger.LogInformation("TransactionCreatedEvent published");

        return transaction.Id;
    }
}
