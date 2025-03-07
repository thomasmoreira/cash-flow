using CashFlow.Application;
using CashFlow.Infraestructure;
using CashFlow.Infraestructure.Common;
using CashFlow.Infraestructure.Persistence;
using CashFlow.Infraestructure.Persistence.Utils;
using CashFlow.Shared;
using CashFlow.Transactions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

StaticLogger.EnsureInitialized(builder.Configuration);

builder.Host.UseSerilog();


builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddMediatr();
builder.Services.AddApplicationMappings();

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
    await DatabaseChecker.WaitForDatabaseAsync(db);
    db.Database.Migrate();
}

app.MapEndpoints();


app.Run();


