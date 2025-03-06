using CashFlow.Application.Common;
using CashFlow.Application.Contracts;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using MapsterMapper;
using MediatR;

namespace CashFlow.Application.Handlers;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResponse<TransactionDto>>
{
    private ITransactionService _transactionService;
    private IMapper _mapper;

    public GetTransactionsQueryHandler(ITransactionService transactionService, IMapper mapper)
    {
        _transactionService = transactionService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalItems) = await _transactionService.GetTransactionsPaginatedAsync(request.Page, request.PageSize);

        var transactions = _mapper.Map<IEnumerable<TransactionDto>>(items);

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        return new PagedResponse<TransactionDto>
        {
            Items = transactions,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
