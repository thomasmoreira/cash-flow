using CashFlow.ApiGateway.Models;
using CashFlow.Application.Commands;
using CashFlow.Application.Common;
using CashFlow.Application.Dtos;
using CashFlow.Infraestructure.Handlers;
using CashFlow.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

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
            options.SwaggerDoc("v1", new OpenApiInfo
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

    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/gateway").RequireAuthorization();

        group.MapPost("/login", (LoginRequest loginRequest, IConfiguration configuration) =>
        {
            if (loginRequest.Username != "cashflow" || loginRequest.Password != "123P@$$word!")
            {
                return Results.Unauthorized();
            }

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, loginRequest.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: appSettings.Jwt.Issuer,
                audience: appSettings.Jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Results.Ok(new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        })
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .WithOpenApi()
        .AllowAnonymous();

        return routes;
    }

    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/gateway").RequireAuthorization();

        group.MapPost("/transactions", async (HttpContext context, CreateTransactionRequest request, IHttpClientFactory clientFactory) =>
        {
            var client = clientFactory.CreateClient("Transactions");

            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/transactions", content);

            context.Response.StatusCode = (int)response.StatusCode;

            await response.Content.CopyToAsync(context.Response.Body);

        })
        .WithName("Create Transaction")
        .Produces<TransactionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Cadastro de Transações";
            operation.Description = "Cria uma nova transação financeira, validando os dados de entrada e publicando um evento para o serviço de consolidação.";
            return operation;
        });


        group.MapGet("/transactions", async (HttpContext context, IHttpClientFactory clientFactory, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        {
            var client = clientFactory.CreateClient("Transactions");

            var response = await client.GetAsync($"/transactions?pageSize={pageSize}&page={page}");

            if (response.Content.Headers.ContentType != null)
            {
                context.Response.ContentType = response.Content.Headers.ContentType.ToString();
            }
            else
            {
                context.Response.ContentType = "application/json";
            }

            context.Response.StatusCode = (int)response.StatusCode;

            await response.Content.CopyToAsync(context.Response.Body);

        })
        .WithName("GetTransactions")
        .Produces<PagedResponse<TransactionDto>>(StatusCodes.Status200OK)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Consulta transações";
            operation.Description = "Retorna uma lista de transações.";
            return operation;
        });

        return routes;
    }

    public static IEndpointRouteBuilder MapConsolidationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/gateway").RequireAuthorization();

        group.MapGet("/daily/consolidation", async (HttpContext context, IHttpClientFactory clientFactory) =>
        {
            var client = clientFactory.CreateClient("Consolidating");

            var response = await client.GetAsync($"/daily/consolidation");

            if (response.Content.Headers.ContentType != null)
            {
                context.Response.ContentType = response.Content.Headers.ContentType.ToString();
            }
            else
            {
                context.Response.ContentType = "application/json";
            }

            context.Response.StatusCode = (int)response.StatusCode;

            await response.Content.CopyToAsync(context.Response.Body);

        })
        .Produces<DailyConsolidationResponse?>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("Get Daily Consolidation")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Consulta o saldo consolidado diário";
            operation.Description = "Retorna o saldo consolidado para a data atual.";
            return operation;
        });

        group.MapGet("/daily/consolidation-report", async (HttpContext context, IHttpClientFactory clientFactory) =>
        {
            var client = clientFactory.CreateClient("Consolidating");

            var response = await client.GetAsync($"/daily/consolidation-report");

            if (response.Content.Headers.ContentType != null)
            {
                context.Response.ContentType = response.Content.Headers.ContentType.ToString();
            }
            else
            {
                context.Response.ContentType = "application/json";
            }

            context.Response.StatusCode = (int)response.StatusCode;

            await response.Content.CopyToAsync(context.Response.Body);


        })
        .Produces<IEnumerable<DailyConsolidationResponse>?>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("Get Daily Consolidation Report")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Relatório de saldos consolidados";
            operation.Description = "Retorna uma lista de saldos consolidados.";
            return operation;
        });

        return routes;

    }
}
