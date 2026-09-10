using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Products.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Queries.GetPagedProducts;

public record GetPagedProductsQuery(int? CategoryId, int PageIndex = 1, int PageSize = 10) : IRequest<PaginatedList<ProductResponse>>;

public class GetPagedProductsQueryHandler : IRequestHandler<GetPagedProductsQuery, PaginatedList<ProductResponse>>
{
    private readonly IProductReadRepository _readRepository;
    private readonly ICurrentUserService _currentUser;

    public GetPagedProductsQueryHandler(IProductReadRepository readRepository, ICurrentUserService currentUser)
    {
        _readRepository = readRepository;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ProductResponse>> Handle(GetPagedProductsQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.IsInRole("Administrator");
        return await _readRepository.GetPagedProductsAsync(
            request.CategoryId,
            request.PageIndex,
            request.PageSize,
            includeHidden: isAdmin,
            cancellationToken);
    }
}
