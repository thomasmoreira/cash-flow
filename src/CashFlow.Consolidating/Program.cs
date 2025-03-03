using CashFlow.Consolidating.Messaging;
using CashFlow.Consolidating.Services;
using CashFlow.Domain.Services;
using CashFlow.Infraestructure.Common;
using MassTransit;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddOpenApi();

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TransactionConsumer>();

        x.UsingRabbitMq((context, config) =>
        {
            config.ConfigureEndpoints(context);

        });
    });


    builder.Services.AddSingleton<IConsolidatingService, ConsolidatingService>();

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

