using CashFlow.Domain.Repositories;
using CashFlow.Infraestructure.Middlewares;
using CashFlow.Infraestructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infraestructure;

public static class Startup
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IConsolidatingRepository, ConsolidatingRepository>();

        return services;
    }

    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}
