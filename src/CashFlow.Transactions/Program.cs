using CashFlow.Application;
using CashFlow.Application.Behaviors;
using CashFlow.Application.Commands;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];
    var jwtKey = builder.Configuration["Jwt:Key"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

    builder.Services.AddAuthorization();

    builder.Services.AddOpenApi();

    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddApplication();
    builder.Services.AddInfraestructure();

    builder.Services.AddMassTransit(bus =>
    {
        bus.UsingRabbitMq((ctx, busConfigurator) =>
        {
            busConfigurator.ConfigureEndpoints(ctx);
        });
    });

    builder.Services.AddMassTransitHostedService();

    
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

