using CashFlow.Infraestructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CashFlow.Infraestructure;

public static class Startup
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}
