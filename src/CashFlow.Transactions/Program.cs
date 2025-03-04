using CashFlow.Application;
using CashFlow.Application.Commands;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddOpenApi();

    builder.Services.AddDatabase(builder.Configuration);


    builder.Services.AddApplication();
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
    })
     .RequireAuthorization();


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

