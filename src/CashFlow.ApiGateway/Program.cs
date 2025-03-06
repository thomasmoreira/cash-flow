using CashFlow.ApiGateway;
using CashFlow.ApiGateway.Models;
using CashFlow.Application;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Handlers;
using CashFlow.Shared;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

StaticLogger.EnsureInitialized(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenPropagationHandler>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddResilientHttpClients(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow API Gateway V1");
    });
}

app.MapLoginEndpoint();
app.MapTransactionEndpoints();
app.MapConsolidationEndpoints();

app.Run();

