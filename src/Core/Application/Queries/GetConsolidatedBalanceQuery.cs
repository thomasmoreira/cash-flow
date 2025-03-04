using CashFlow.Application.Models;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetConsolidatedBalanceQuery : IRequest<BalanceConsolidationResponse>
{
    public DateTime Date { get; }

    public GetConsolidatedBalanceQuery(DateTime date)
    {        
        Date = date.Date;
    }
}
