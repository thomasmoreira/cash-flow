using CashFlow.Application.Dtos;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetConsolidatedBalanceQuery : IRequest<BalanceConsolidationDto>
{
    public DateTime Date { get; }

    public GetConsolidatedBalanceQuery(DateTime date)
    {        
        Date = date.Date;
    }
}
