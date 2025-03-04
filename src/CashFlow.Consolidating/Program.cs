using CashFlow.Application;
using CashFlow.Application.Queries;
using CashFlow.Consolidating.Messaging;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Use as mesmas configurações definidas no API Gateway:
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];
    var jwtKey = builder.Configuration["Jwt:Key"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

    builder.Services.AddAuthorization();

    builder.Services.AddOpenApi();

    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TransactionConsumer>();

        x.UsingRabbitMq((context, config) =>
        {
            config.ConfigureEndpoints(context);

        });
    });

    builder.Services.AddApplication();
    builder.Services.AddRepositories();
    builder.Services.AddServices();

    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapGet("/daily-consolidation", async ([FromQuery] DateTime date, IMediator mediator) =>
    {
        var query = new GetConsolidatedBalanceQuery(date);
        return await mediator.Send(query);
    });

    app.MapGet("/balance-consolidation-report", async (IMediator mediator) =>
    {
        var query = new GetConsolidatedBalanceReportQuery();
        return await mediator.Send(query);
    });

    app.Run();
}
catch (Exception ex)
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}

