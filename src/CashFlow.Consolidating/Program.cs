using CashFlow.Application;
using CashFlow.Consolidating.Messaging;
using CashFlow.Consolidating.Services;
using CashFlow.Domain.Services;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

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
    builder.Services.AddInfraestructure();
    builder.Services.AddScoped<IConsolidatingService, ConsolidatingService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

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

