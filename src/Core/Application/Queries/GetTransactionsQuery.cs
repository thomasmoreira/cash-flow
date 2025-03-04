using CashFlow.Application.Common;
using CashFlow.Application.Dtos;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetTransactionsQuery : IRequest<PagedResponse<TransactionDto>>
{
    public int Page { get; }
    public int PageSize { get; }

    public GetTransactionsQuery(int page, int pageSize)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize < 1 ? 10 : pageSize;
    }
}
