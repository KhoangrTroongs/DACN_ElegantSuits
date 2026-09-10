using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Products.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsQuery(string Keyword, int PageIndex = 1, int PageSize = 10) : IRequest<PaginatedList<ProductResponse>>;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, PaginatedList<ProductResponse>>
{
    private readonly IProductReadRepository _readRepository;
    private readonly ICurrentUserService _currentUser;

    public SearchProductsQueryHandler(IProductReadRepository readRepository, ICurrentUserService currentUser)
    {
        _readRepository = readRepository;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ProductResponse>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.IsInRole("Administrator");
        return await _readRepository.SearchProductsAsync(
            request.Keyword,
            request.PageIndex,
            request.PageSize,
            includeHidden: isAdmin,
            cancellationToken);
    }
}
