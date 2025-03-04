using CashFlow.Application;
using CashFlow.Application.Models;
using CashFlow.Application.Queries;
using CashFlow.Consolidating.Messaging;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Shared;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    var result = await mediator.Send(query);
    return Results.Json(result);

})
 .Produces<BalanceConsolidationResponse?>(200)
 .RequireAuthorization();

app.MapGet("/balance-consolidation-report", async (IMediator mediator) =>
{
    var query = new GetConsolidatedBalanceReportQuery();
    var result = await mediator.Send(query);
    return Results.Json(result);

})
 .Produces<IEnumerable<BalanceConsolidationResponse>?>(200)
 .RequireAuthorization();

app.Run();


