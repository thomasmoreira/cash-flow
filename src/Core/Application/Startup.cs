using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CashFlow.Application;

public static class Startup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services
            .AddValidatorsFromAssembly(assembly)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            
        return services;
    }
}
