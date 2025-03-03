using CashFlow.Consolidating.Messaging;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionConsumer>();   

    x.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
        //config.Host(new Uri("amqp://guest:guest@rabbitmq:5672/"));

    });
});

// Caso deseje expor um endpoint para consulta do saldo consolidado:
//builder.Services.AddSingleton<ConsolidacaoService>(); // Serviço que gerencia os saldos

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
