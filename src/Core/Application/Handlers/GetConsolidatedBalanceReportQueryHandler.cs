using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MassTransit.Initializers;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetConsolidatedBalanceReportQueryHandler : IRequestHandler<GetConsolidatedBalanceReportQuery, IEnumerable<BalanceConsolidationDto>?>
{
    private readonly IConsolidationService _service;

    public GetConsolidatedBalanceReportQueryHandler(IConsolidationService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<BalanceConsolidationDto>?> Handle(GetConsolidatedBalanceReportQuery request, CancellationToken cancellationToken)
    {

        var balanceConsolidationReport = await _service.ConsolidateBalanceReportAsync();

        return balanceConsolidationReport?.Select(static r => new BalanceConsolidationDto { Date = r.Date, Total = r.Amount }).ToList();

        
    }
}
