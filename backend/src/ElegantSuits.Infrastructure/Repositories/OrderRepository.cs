using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Orders.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToOrderDTO);
    }

    public async Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToOrderDTO);
    }

    public async Task<OrderDTO?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order != null ? MapToOrderDTO(order) : null;
    }

    public async Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO dto, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi đặt hàng.");
        }

        foreach (var item in cart.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            if (product == null || product.Quantity < item.Quantity)
            {
                throw new InvalidOperationException($"Sản phẩm '{item.ProductName}' không đủ số lượng tồn kho.");
            }
            product.Quantity -= item.Quantity;
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            ShippingAddress = dto.ShippingAddress,
            Notes = dto.Notes,
            Status = OrderStatus.Pending,
            PaymentMethod = dto.PaymentMethod ?? "COD",
            PaymentStatus = PaymentStatus.Pending,
            TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity),
            TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
            CouponCode = dto.CouponCode,
            OrderDetails = cart.Items.Select(item => new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Size = item.Size
            }).ToList()
        };

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetOrderByIdAsync(order.Id, cancellationToken))!;
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order == null) return false;

        order.Status = status;
        order.OrderStatus = status;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static OrderDTO MapToOrderDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User?.FullName ?? order.User?.UserName,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
            {
                Id = od.Id,
                OrderId = od.OrderId,
                ProductId = od.ProductId,
                ProductName = od.Product?.Name ?? "",
                Price = od.Price,
                Quantity = od.Quantity,
                Size = od.Size,
                ProductImageUrl = od.Product?.ImageUrl
            }).ToList() ?? new List<OrderDetailDTO>()
        };
    }
}
