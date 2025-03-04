using CashFlow.Application;
using CashFlow.Application.Commands;
using CashFlow.Application.Models;
using CashFlow.Application.Queries;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;



try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    StaticLogger.EnsureInitialized(builder.Configuration);
    Log.Information("Server Booting Up...");

    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddOpenApi();

    builder.Services.AddDatabase(builder.Configuration);

    builder.Services.AddMediatr();

    builder.Services.AddRepositories();

    builder.Services.AddServices();

    builder.Services.AddMassTransitConfiguration();

    builder.Services.AddPipelineBehaviors();

    var app = builder.Build();
    app.UseExceptionHandlingMiddleware();

    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }

    app.MapPost("/transactions", async (CreateTransactionCommand command, IMediator mediator) =>
    {
        return await mediator.Send(command);

    }).RequireAuthorization();

    app.MapGet("/transactions", async (IMediator mediator, [FromQuery] int pageSize = 10, [FromQuery] int page = 1) =>
    {
        var query = new GetTransactionsQuery(page, pageSize);
        var result = await mediator.Send(query);
        return Results.Json(result);

    })
 .Produces<BalanceConsolidationResponse?>(200)
 .RequireAuthorization();


    app.Run();
}
catch (Exception ex)
{
    
    Log.Fatal(ex, "Unhandled exception");
}
finally
{    
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}

