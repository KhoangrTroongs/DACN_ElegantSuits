using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Orders.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(string UserId, CreateOrderDTO DTO) : IRequest<OrderDTO>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.DTO.ShippingAddress).NotEmpty().WithMessage("Địa chỉ giao hàng không được để trống.");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDTO>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDTO> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        return await _orderRepository.CreateOrderAsync(command.UserId, command.DTO, cancellationToken);
    }
}
