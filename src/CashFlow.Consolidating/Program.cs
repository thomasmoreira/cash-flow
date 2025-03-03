using CashFlow.Application;
using CashFlow.Consolidating.Messaging;
using CashFlow.Consolidating.Services;
using CashFlow.Domain.Services;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using MassTransit;
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

    // Use as mesmas configurações definidas no API Gateway:
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

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TransactionConsumer>();

        x.UsingRabbitMq((context, config) =>
        {
            config.ConfigureEndpoints(context);

        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfraestructure();
    builder.Services.AddScoped<IConsolidatingService, ConsolidatingService>();

    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

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

