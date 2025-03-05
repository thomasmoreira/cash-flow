using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetConsolidatedBalanceQueryHandler : IRequestHandler<GetConsolidatedBalanceQuery, BalanceConsolidationDto>
{
    private readonly IConsolidationService _service;

    public GetConsolidatedBalanceQueryHandler(IConsolidationService service)
    {
        _service = service;
    }

    public async Task<BalanceConsolidationDto> Handle(GetConsolidatedBalanceQuery request, CancellationToken cancellationToken)
    {

        var consolidation = await _service.DailyConsolidationAsync(request.Date);            

        if (consolidation == null)
        {            
            return new BalanceConsolidationDto { Date = request.Date, Total = 0 };
        }

        return new BalanceConsolidationDto
        {
            Date = consolidation.Date,
            Total = consolidation.Amount
        };
    }
}
