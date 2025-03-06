using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MapsterMapper;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetDailyConsolidationReportQueryHandler : IRequestHandler<GetConsolidatedBalanceReportQuery, IEnumerable<DailyConsolidationReportDto>?>
{
    private readonly IConsolidationService _service;
    private readonly IMapper _mapper;

    public GetDailyConsolidationReportQueryHandler(IConsolidationService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DailyConsolidationReportDto>?> Handle(GetConsolidatedBalanceReportQuery request, CancellationToken cancellationToken)
    {

        var result = await _service.DailyConsolidationReportAsync();

        var report = _mapper.Map<IEnumerable<DailyConsolidationReportDto>>(result);

        return report;

        
    }
}
