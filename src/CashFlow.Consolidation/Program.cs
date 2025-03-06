using CashFlow.Application;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using CashFlow.Consolidating.Messaging;
using CashFlow.Consolidation;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Shared;
using MassTransit;
using MediatR;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

StaticLogger.EnsureInitialized(builder.Configuration);

builder.Host.UseSerilog();


builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddDatabase(builder.Configuration);


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionConsumer>();

    x.UsingRabbitMq((context, config) =>
    {
        config.UseMessageRetry(r =>
        {
            r.Interval(3, TimeSpan.FromSeconds(5));

        });
        config.ConfigureEndpoints(context);

    });
});

builder.Services.AddMediatr();
builder.Services.AddApplicationMappings();
builder.Services.AddRepositories();
builder.Services.AddServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapEndpoints();

app.Run();


