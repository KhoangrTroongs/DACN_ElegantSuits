using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Cart.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(string UserId, UpdateCartItemDTO DTO) : IRequest<CartDTO?>;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.DTO.CartItemId).GreaterThan(0).WithMessage("CartItemId must be greater than 0.");
    }
}

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDTO?>
{
    private readonly ICartRepository _cartRepository;

    public UpdateCartItemCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartDTO?> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        return await _cartRepository.UpdateCartItemAsync(command.UserId, command.DTO, cancellationToken);
    }
}
