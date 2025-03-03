using CashFlow.Domain.Events;
using CashFlow.Domain.Services;
using MassTransit;

namespace CashFlow.Consolidating.Messaging;

public class TransactionConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly ILogger<TransactionConsumer> _logger;
    private readonly IConsolidatingService _consolidatingService;

    public TransactionConsumer(ILogger<TransactionConsumer> logger, IConsolidatingService consolidatingService)
    {
        _logger = logger;
        _consolidatingService = consolidatingService;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var @event = context.Message;

        await _consolidatingService.Consolidate(@event.TransactionId);

        _logger.LogInformation("Processing transaction {TransactionId}", @event.TransactionId);

        Console.WriteLine($"Processando lançamento: {@event}");

        _logger.LogInformation("Transaction {TransactionId} processed", @event.TransactionId);

        await Task.CompletedTask;
    }
}
