using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MediatR;

namespace CashFlow.Consolidation;

public static class Startup
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("daily").RequireAuthorization();


        group.MapGet("/consolidation", async (IMediator mediator) =>
        {
            var query = new GetConsolidatedBalanceQuery();
            var result = await mediator.Send(query);
            return Results.Json(result);

        })
         .Produces<DailyConsolidationReportDto?>(200);

        group.MapGet("/consolidation-report", async (IMediator mediator) =>
        {
            var query = new GetConsolidatedBalanceReportQuery();
            var result = await mediator.Send(query);
            return Results.Json(result);

        })
         .Produces<IEnumerable<DailyConsolidationReportDto>?>(200);

        return routes;
    }
}
