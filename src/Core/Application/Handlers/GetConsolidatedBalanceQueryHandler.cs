using CashFlow.Application.Models;
using CashFlow.Application.Queries;
using CashFlow.Domain.Services;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetConsolidatedBalanceQueryHandler : IRequestHandler<GetConsolidatedBalanceQuery, BalanceConsolidationResponse>
{
    private readonly IConsolidatingService _service;

    public GetConsolidatedBalanceQueryHandler(IConsolidatingService service)
    {
        _service = service;
    }

    public async Task<BalanceConsolidationResponse> Handle(GetConsolidatedBalanceQuery request, CancellationToken cancellationToken)
    {        
        var consolidation = await _service.DailyConsolidationAsync(request.Date);            

        if (consolidation == null)
        {            
            return new BalanceConsolidationResponse { Date = request.Date, Total = 0 };
        }

        return new BalanceConsolidationResponse
        {
            Date = consolidation.Date,
            Total = consolidation.Amount
        };
    }
}
