using CashFlow.Application.Commands;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Transactions;

public static class Startup
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("").RequireAuthorization();


        group.MapPost("/transactions", async (CreateTransactionCommand command, IMediator mediator) =>
        {
            return await mediator.Send(command);

        });

        group.MapGet("/transactions", async (IMediator mediator, [FromQuery] int pageSize = 10, [FromQuery] int page = 1) =>
        {
            var query = new GetTransactionsQuery(page, pageSize);
            var result = await mediator.Send(query);
            return Results.Json(result);

        })
        .Produces<DailyConsolidationReportDto?>(200);

        return routes;
    }
}
