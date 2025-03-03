using CashFlow.Domain.Events;
using CashFlow.Domain.Services;
using CashFlow.Infraestructure.Extensions;
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

        _logger.LogInformation("Consuming TransactionCreatedEvent: {TransactionCreatedEvent}", @event.ToJson());
        
        await _consolidatingService.Consolidate(@event.TransactionId);

        _logger.LogInformation("TransactionConsolidatedEvent consumed");

        await Task.CompletedTask;
    }
}
