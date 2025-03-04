using CashFlow.Application.Models;
using CashFlow.Application.Queries;
using CashFlow.Domain.Services;
using MassTransit.Initializers;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetConsolidatedBalanceReportQueryHandler : IRequestHandler<GetConsolidatedBalanceReportQuery, IEnumerable<BalanceConsolidationResponse>?>
{
    private readonly IConsolidatingService _service;

    public GetConsolidatedBalanceReportQueryHandler(IConsolidatingService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<BalanceConsolidationResponse>?> Handle(GetConsolidatedBalanceReportQuery request, CancellationToken cancellationToken)
    {

        var balanceConsolidationReport = await _service.ConsolidateBalanceReportAsync();

        return balanceConsolidationReport?.Select(static r => new BalanceConsolidationResponse { Date = r.Date, Total = r.Amount }).ToList();

        
    }
}
