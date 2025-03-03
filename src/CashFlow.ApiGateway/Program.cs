using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configura��o do Serilog para Seq
Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();


// Registro dos HttpClients para se comunicar com os servi�os
builder.Services.AddHttpClient("Transactions", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001"); // Porta do servi�o de Lan�amentos
});
builder.Services.AddHttpClient("Consolidating", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002"); // Porta do servi�o de Consolida��o
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Expondo endpoint de cria��o de lan�amento � o gateway repassa a requisi��o para o servi�o de lan�amentos
app.MapPost("/gateway/transactions", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Transactions");
    // Repasse da requisi��o: voc� pode ler o body e encaminhar ou usar um proxy reverso simples
    var response = await client.PostAsync("/transactions", null/*context.Request.Body*/);
    context.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(context.Response.Body);
});

// Expondo endpoint de consulta do consolidado
app.MapGet("/gateway/consolidating", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");
    // Transfere os par�metros da query string, se necess�rio
    var data = context.Request.Query["data"].ToString();
    var response = await client.GetAsync($"/consolidating?data={data}");
    context.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(context.Response.Body);
});

app.Run();

