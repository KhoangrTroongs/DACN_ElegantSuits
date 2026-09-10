using ElegantSuits.Application.Features.Orders.Contracts;
using ElegantSuits.Domain.Enums;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<OrderDTO>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDTO?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default);
}
