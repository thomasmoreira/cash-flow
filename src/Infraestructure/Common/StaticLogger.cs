using CashFlow.Shared;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CashFlow.Infraestructure.Common
{
    public static class StaticLogger
    {
        public static void EnsureInitialized(IConfiguration configuration)
        {
            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            if (Log.Logger is not Serilog.Core.Logger)
            {
                Log.Logger = new LoggerConfiguration()                    
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Seq(appSettings.Seq.Url)
                    .CreateLogger();
            }
        }
    }
}
