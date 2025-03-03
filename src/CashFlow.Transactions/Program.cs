using CashFlow.Application;
using CashFlow.Application.Behaviors;
using CashFlow.Application.Commands;
using CashFlow.Domain.Repositories;
using CashFlow.Infraestructure.Persistence;
using CashFlow.Infraestructure.Persistence.Repositories;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddApplication();

builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((ctx, busConfigurator) =>
    {
        //busConfigurator.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        busConfigurator.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Endpoint para registrar lançamentos
app.MapPost("/transactions", async (CreateTransactionCommand command, IMediator mediator) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/transactions/{id}", new { Id = id });
});

app.Run();

