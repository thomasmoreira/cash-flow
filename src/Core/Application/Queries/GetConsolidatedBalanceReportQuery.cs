using CashFlow.Application.Models;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetConsolidatedBalanceReportQuery : IRequest<IEnumerable<BalanceConsolidationResponse>>
{    

}
