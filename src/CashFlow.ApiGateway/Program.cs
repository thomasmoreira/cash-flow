using CashFlow.ApiGateway;
using CashFlow.ApiGateway.Models;
using CashFlow.Application;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Handlers;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenPropagationHandler>();

builder.Services.Configure<ServiceUrlsOptions>(builder.Configuration.GetSection("ServiceUrls"));

builder.Services.AddJwtAuthentication(builder.Configuration);

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];


//builder.Services.AddHttpClients();
builder.Services.AddResilientHttpClients(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow API Gateway V1");
    });
}

app.MapPost("/login", (LoginRequest loginRequest) =>
{    
    if (loginRequest.Username != "usuario" || loginRequest.Password != "senha")
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, loginRequest.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token)
    });
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/gateway/transactions", async (HttpContext context, CreateTransactionRequest request, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Transactions");

    var json = System.Text.Json.JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("/transactions", content);

    // Define o status code da resposta
    context.Response.StatusCode = (int)response.StatusCode;

    // Opcional: copia o conteúdo da resposta, se necessário
    await response.Content.CopyToAsync(context.Response.Body);

})
.WithName("Create Transaction")
.Produces<TransactionResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.WithOpenApi()
.RequireAuthorization();


app.MapGet("/gateway/daily-consolidation", async (HttpContext context, DateTime date, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");
    
    var response = await client.GetAsync($"/daily-consolidation?date={date}");

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);
    
})
.Produces<DailyConsolidationResponse?>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("Get Daily Consolidation");

app.MapGet("/gateway/balance-consolidation-report", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");

    var response = await client.GetAsync($"/balance-consolidation-report");

    context.Response.StatusCode = (int)response.StatusCode;

    await response.Content.CopyToAsync(context.Response.Body);


})
.Produces<IEnumerable<DailyConsolidationResponse>?>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("Get Daily Consolidation Report");

app.Run();

