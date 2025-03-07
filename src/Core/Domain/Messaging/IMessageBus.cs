namespace CashFlow.Domain.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T mensagem);
}
