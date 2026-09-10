using ElegantSuits.Application.Common.Interfaces;
using MediatR;

namespace ElegantSuits.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(string UserId) : IRequest<bool>;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly ICartRepository _cartRepository;

    public ClearCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<bool> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        return await _cartRepository.ClearCartAsync(command.UserId, cancellationToken);
    }
}
