using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Orders.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery : IRequest<IEnumerable<OrderDTO>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDTO>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDTO>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetAllOrdersAsync(cancellationToken);
    }
}
