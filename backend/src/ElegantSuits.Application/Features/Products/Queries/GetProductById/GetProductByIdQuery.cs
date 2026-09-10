using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductResponse?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    private readonly IProductReadRepository _readRepository;
    private readonly ICurrentUserService _currentUser;

    public GetProductByIdQueryHandler(IProductReadRepository readRepository, ICurrentUserService currentUser)
    {
        _readRepository = readRepository;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.IsInRole("Administrator");
        var product = await _readRepository.GetProductByIdAsync(request.Id, includeHidden: isAdmin, cancellationToken);
        return product;
    }
}
