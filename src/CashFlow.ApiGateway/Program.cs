using CashFlow.ApiGateway.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Seq
Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Services.AddHttpClient("Transactions", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});
builder.Services.AddHttpClient("Consolidating", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002");
});

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{    
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CashFlow API Gateway",
        Version = "v1",
        Description = "API Gateway unificando os serviços de Transactions e Consolidation."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow API Gateway V1");
    });
}

app.MapPost("/gateway/transactions", async (CreateTransactionRequest request, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Transactions");

    var json = System.Text.Json.JsonSerializer.Serialize(request);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    var result = await client.PostAsync("/transactions", content);
    return result.Content;
})
.WithName("CriarLancamento")
.Produces<TransactionResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(); ;

app.MapGet("/gateway/consolidating", async (DateTime date, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");
    
    var response = await client.GetAsync($"/consolidating?data={date}");
    return response.Content;
    
})
.Produces<ConsolidatingResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("Get Consolidating");

app.Run();

