using CashFlow.Domain.Repositories;
using CashFlow.Infraestructure.Middlewares;
using CashFlow.Infraestructure.Persistence;
using CashFlow.Infraestructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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


    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        return services;
    }

    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services)
    {
        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.ConfigureEndpoints(ctx);
            });
        });
        services.AddMassTransitHostedService();
        return services;
    }


    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}
