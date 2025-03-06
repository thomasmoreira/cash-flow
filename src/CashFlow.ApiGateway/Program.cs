using CashFlow.ApiGateway;
using CashFlow.ApiGateway.Models;
using CashFlow.Application;
using CashFlow.Application.Common;
using CashFlow.Application.Dtos;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Handlers;
using CashFlow.Shared;
using Microsoft.AspNetCore.Mvc;
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

app.MapPost("/login", (LoginRequest loginRequest, IConfiguration configuration) =>
{
    if (loginRequest.Username != "cashflow" || loginRequest.Password != "123P@$$word!")
    {
        return Results.Unauthorized();
    }

    var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
    
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, loginRequest.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: appSettings.Jwt.Issuer,
        audience: appSettings.Jwt.Audience,
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);

    return Results.Ok(new LoginResponse
    {
        Token = new JwtSecurityTokenHandler().WriteToken(token)
    });
})
.WithName("Login")
.Produces<LoginResponse>(StatusCodes.Status200OK)
.WithOpenApi()
.AllowAnonymous();


app.MapPost("/gateway/transactions", async (HttpContext context, CreateTransactionRequest request, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Transactions");

    var json = System.Text.Json.JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("/transactions", content);

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);

})
.WithName("Create Transaction")
.Produces<TransactionResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/gateway/consolidation/daily-consolidation", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");
    
    var response = await client.GetAsync($"/daily-consolidation");

    if (response.Content.Headers.ContentType != null)
    {
        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
    }
    else
    {
        context.Response.ContentType = "application/json";
    }

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);
    
})
.Produces<DailyConsolidationResponse?>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("Get Daily Consolidation")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/gateway/consolidation/balance-consolidation-report", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");

    var response = await client.GetAsync($"/balance-consolidation-report");

    if (response.Content.Headers.ContentType != null)
    {
        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
    }
    else
    {
        context.Response.ContentType = "application/json";
    }

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);


})
.Produces<IEnumerable<DailyConsolidationResponse>?>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("Get Daily Consolidation Report")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/gateway/transactions", async (HttpContext context, IHttpClientFactory clientFactory, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
{
    var client = clientFactory.CreateClient("Transactions");

    var response = await client.GetAsync($"/transactions?pageSize={pageSize}&page={page}");

    if (response.Content.Headers.ContentType != null)
    {
        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
    }
    else
    {
        context.Response.ContentType = "application/json";
    }

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);

})
.WithName("GetTransactions")
.Produces<PagedResponse<TransactionDto>>(StatusCodes.Status200OK)
.WithOpenApi();

app.Run();

