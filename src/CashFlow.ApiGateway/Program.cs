using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para Seq
Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();


// Registro dos HttpClients para se comunicar com os serviços
builder.Services.AddHttpClient("Transactions", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001"); // Porta do serviço de Lançamentos
});
builder.Services.AddHttpClient("Consolidating", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002"); // Porta do serviço de Consolidação
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Expondo endpoint de criação de lançamento – o gateway repassa a requisição para o serviço de lançamentos
app.MapPost("/gateway/transactions", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Transactions");
    // Repasse da requisição: você pode ler o body e encaminhar ou usar um proxy reverso simples
    var response = await client.PostAsync("/transactions", null/*context.Request.Body*/);
    context.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(context.Response.Body);
});

// Expondo endpoint de consulta do consolidado
app.MapGet("/gateway/consolidating", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Consolidating");
    // Transfere os parâmetros da query string, se necessário
    var data = context.Request.Query["data"].ToString();
    var response = await client.GetAsync($"/consolidating?data={data}");
    context.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(context.Response.Body);
});

app.Run();

