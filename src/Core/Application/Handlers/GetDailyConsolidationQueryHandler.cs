using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MapsterMapper;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetDailyConsolidationQueryHandler : IRequestHandler<GetConsolidatedBalanceQuery, DailyConsolidationReportDto>
{
    private readonly IConsolidationService _service;
    private readonly IMapper _mapper;

    public GetDailyConsolidationQueryHandler(IConsolidationService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<DailyConsolidationReportDto> Handle(GetConsolidatedBalanceQuery request, CancellationToken cancellationToken)
    {

        var result = await _service.DailyConsolidationAsync(request.Date);

        var dailyConsolidation = _mapper.Map<DailyConsolidationReportDto>(result);

        return dailyConsolidation ?? new DailyConsolidationReportDto { Amount = 0, Date = request.Date};

    }
}
