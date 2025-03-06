using CashFlow.Application.Dtos;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetConsolidatedBalanceReportQuery : IRequest<IEnumerable<DailyConsolidationReportDto>>
{    

}
