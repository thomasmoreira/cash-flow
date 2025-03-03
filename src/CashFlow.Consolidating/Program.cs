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
        config.Host(new Uri("rabbitmq://localhost"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        //config.ReceiveEndpoint("send-command", e =>
        //{
        //    e.Consumer<SenderService>(context);
        //});
        //config.ReceiveEndpoint("publish-event", e =>
        //{
        //    e.Consumer<PublisherService>(context);
        //});
        //config.ReceiveEndpoint("request-response", e =>
        //{
        //    e.Consumer<RequestResponseService>(context);
        //});
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
