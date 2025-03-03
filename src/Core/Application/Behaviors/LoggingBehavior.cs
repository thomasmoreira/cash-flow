using CashFlow.Infraestructure.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        
        logger.LogInformation("Handling request {CorrelationID}: {Request}", correlationId, request.ToJson());

        var response = await next();        

        logger.LogInformation("Response for {Correlation}: {Response}", correlationId, response?.ToJson());

        return response;
    }
}
