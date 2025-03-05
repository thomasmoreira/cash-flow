using CashFlow.Application.Dtos;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetConsolidatedBalanceQuery : IRequest<BalanceConsolidationDto>
{    
    public DateTime Date { get; private set; }
    public GetConsolidatedBalanceQuery()
    {
        Date = DateTime.Now.Date;
    }
}
