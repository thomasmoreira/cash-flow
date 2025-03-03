using CashFlow.Application;
using CashFlow.Application.Behaviors;
using CashFlow.Application.Commands;
using CashFlow.Domain.Repositories;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Persistence;
using CashFlow.Infraestructure.Persistence.Repositories;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
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
                busConfigurator.ConfigureEndpoints(ctx);
            });
        });

        builder.Services.AddMassTransitHostedService();

        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var app = builder.Build();
        app.UseExceptionHandlingMiddleware();

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

        });


        app.Run();
    }
}