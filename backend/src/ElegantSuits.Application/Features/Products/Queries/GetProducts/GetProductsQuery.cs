using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(int? CategoryId) : IRequest<IReadOnlyList<ProductResponse>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductResponse>>
{
    private readonly IProductReadRepository _readRepository;
    private readonly ICurrentUserService _currentUser;

    public GetProductsQueryHandler(IProductReadRepository readRepository, ICurrentUserService currentUser)
    {
        _readRepository = readRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.IsInRole("Administrator");
        return await _readRepository.GetProductsByCategoryAsync(request.CategoryId, includeHidden: isAdmin, cancellationToken);
    }
}
