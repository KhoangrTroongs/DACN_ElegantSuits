using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Orders.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Orders.Queries.GetUserOrders;

public record GetUserOrdersQuery(string UserId) : IRequest<IEnumerable<OrderDTO>>;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IEnumerable<OrderDTO>>
{
    private readonly IOrderRepository _orderRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDTO>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetUserOrdersAsync(request.UserId, cancellationToken);
    }
}
