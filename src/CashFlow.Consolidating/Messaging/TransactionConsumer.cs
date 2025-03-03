using CashFlow.Domain.Events;
using MassTransit;

namespace CashFlow.Consolidating.Messaging;

public class TransactionConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly ILogger<TransactionConsumer> _logger;

    public TransactionConsumer(ILogger<TransactionConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Processing transaction {TransactionId}", @event);

        Console.WriteLine($"Processando lançamento: {@event}");

        _logger.LogInformation("Transaction { TransactionId } processed", @event);

        await Task.CompletedTask;
    }
}
