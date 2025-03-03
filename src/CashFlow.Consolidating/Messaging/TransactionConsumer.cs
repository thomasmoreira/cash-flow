using CashFlow.Domain.Entities;
using MassTransit;
using RabbitMQ.Client;

namespace CashFlow.Consolidating.Messaging;

public class TransactionConsumer : IConsumer<Transaction>
{
    public async Task Consume(ConsumeContext<Transaction> context)
    {
        var lancamento = context.Message;
        // Lógica para processar o lançamento
        Console.WriteLine($"Processando lançamento: {lancamento.Id}");
        // Aqui você pode atualizar o saldo, persistir dados, etc.

        await Task.CompletedTask;
    }
}
