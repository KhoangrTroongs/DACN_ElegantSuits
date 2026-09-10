using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Cart.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(string UserId) : IRequest<CartDTO?>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDTO?>
{
    private readonly ICartRepository _cartRepository;

    public GetCartQueryHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartDTO?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        return await _cartRepository.GetCartAsync(request.UserId, cancellationToken);
    }
}
