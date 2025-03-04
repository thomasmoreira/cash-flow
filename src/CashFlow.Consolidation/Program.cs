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

    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddOpenApi();

    builder.Services.AddDatabase(builder.Configuration);


    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TransactionConsumer>();

        x.UsingRabbitMq((context, config) =>
        {
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
        return await mediator.Send(query);

    }).RequireAuthorization();

    app.MapGet("/balance-consolidation-report", async (IMediator mediator) =>
    {
        var query = new GetConsolidatedBalanceReportQuery();
        return await mediator.Send(query);

    }).RequireAuthorization();

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

