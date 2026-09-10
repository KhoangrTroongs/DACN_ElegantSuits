using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Cart.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(string UserId, AddToCartDTO DTO) : IRequest<CartDTO>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.DTO.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0.");
        RuleFor(x => x.DTO.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDTO>
{
    private readonly ICartRepository _cartRepository;

    public AddToCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartDTO> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        return await _cartRepository.AddToCartAsync(command.UserId, command.DTO, cancellationToken);
    }
}
