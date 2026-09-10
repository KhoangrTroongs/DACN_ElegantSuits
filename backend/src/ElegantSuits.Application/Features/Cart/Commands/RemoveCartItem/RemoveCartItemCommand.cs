using ElegantSuits.Application.Common.Interfaces;
using MediatR;

namespace ElegantSuits.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(string UserId, int CartItemId) : IRequest<bool>;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly ICartRepository _cartRepository;

    public RemoveCartItemCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<bool> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        return await _cartRepository.RemoveCartItemAsync(command.UserId, command.CartItemId, cancellationToken);
    }
}
