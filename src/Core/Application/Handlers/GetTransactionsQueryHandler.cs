using CashFlow.Application.Common;
using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResponse<TransactionDto>>
{
    private ITransactionService _transactionService;

    public GetTransactionsQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<PagedResponse<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalItems) = await _transactionService.GetTransactionsPaginatedAsync(request.Page, request.PageSize);

        var dtos = items.Select(t => new TransactionDto
        {
            Id = t.Id,
            Date = t.Date,
            Type = t.Type.GetDisplayName(),
            Amount = t.Amount,
            Description = t.Description
        });

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        return new PagedResponse<TransactionDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
