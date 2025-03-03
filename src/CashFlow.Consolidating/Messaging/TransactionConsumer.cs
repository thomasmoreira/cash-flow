using CashFlow.Domain.Events;
using MassTransit;

namespace CashFlow.Consolidating.Messaging;

public class TransactionConsumer : IConsumer<TransactionCreatedEvent>
{
    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var @event = context.Message;

        Console.WriteLine($"Processando lançamento: {@event}");

        await Task.CompletedTask;
    }
}
