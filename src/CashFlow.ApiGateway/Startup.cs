using CashFlow.Infraestructure.Handlers;
using CashFlow.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

namespace CashFlow.ApiGateway;

public static class Startup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("Transactions", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
            client.BaseAddress = new Uri(options.Transactions);
        })
         .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddHttpClient("Consolidating", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
            client.BaseAddress = new Uri(options.Consolidation);
        })
         .AddHttpMessageHandler<TokenPropagationHandler>();

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "CashFlow API Gateway",
                Version = "v1",
                Description = "API Gateway unificando os serviços de Transactions e Consolidation."
            });

            // Define a segurança usando o esquema Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Insira 'Bearer' [espaço] e o seu token JWT\nExemplo: Bearer eyJhbGciOiJIUzI1NiIsInR...",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Requer o esquema Bearer para todos os endpoints
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
        });

        return services;
    }

    public static IServiceCollection AddResilientHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        
        services.AddHttpClient("Transactions", (sp, client) =>
        {
            //var serviceOptions = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
            client.BaseAddress = new Uri(appSettings.ServiceUrls.Transactions);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy)
        .AddHttpMessageHandler<TokenPropagationHandler>();



        services.AddHttpClient("Consolidating", (sp, client) =>
        {
            //var serviceOptions = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
            client.BaseAddress = new Uri(appSettings.ServiceUrls.Consolidation);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy)
        .AddHttpMessageHandler<TokenPropagationHandler>();

        return services;
    }
}
