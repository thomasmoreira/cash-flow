using CashFlow.Consolidating.Messaging;
using CashFlow.Consolidating.Services;
using CashFlow.Domain.Services;
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


builder.Services.AddSingleton<IConsolidatingService, ConsolidatingService>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
