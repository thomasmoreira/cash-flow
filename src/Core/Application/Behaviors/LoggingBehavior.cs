using MediatR;

namespace CashFlow.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Log antes de executar o handler
        Console.WriteLine($"Iniciando {typeof(TRequest).Name}");

        var response = await next();

        // Log após execução
        Console.WriteLine($"Finalizando {typeof(TRequest).Name}");

        return response;
    }
}
