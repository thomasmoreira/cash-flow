using CashFlow.Application.Commands;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Repositories;
using MassTransit;
using MediatR;

namespace CashFlow.Application.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private ITransactionRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateTransactionCommandHandler(ITransactionRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var type = (TransactionType)request.Type;
        var transaction = new Transaction(Guid.NewGuid(), request.Date, type, request.Amount, request.Description);

        await _repository.AddAsync(transaction);

        // Publica o lançamento para a fila via MassTransit
        await _publishEndpoint.Publish(transaction, cancellationToken);

        return transaction.Id;
    }
}
