using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            // Lấy tất cả đơn hàng, bao gồm thông tin người dùng và chi tiết đơn hàng (kèm sản phẩm)
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToOrderDTO);
        }

        public async Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId)
        {
            // Lấy tất cả đơn hàng của một user cụ thể
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToOrderDTO);
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int id)
        {
            // Lấy đơn hàng theo Id, bao gồm thông tin user và chi tiết đơn hàng
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order != null ? MapToOrderDTO(order) : null;
        }

        public async Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO orderDto)
        {
            // Lấy giỏ hàng của user, bao gồm các sản phẩm trong giỏ
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi đặt hàng.");
            }

            // Kiểm tra số lượng tồn kho của từng sản phẩm trong giỏ hàng
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Sản phẩm '{item.ProductName}' không đủ số lượng.");
                }
            }

            // Tạo mới đơn hàng
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShippingAddress = orderDto.ShippingAddress,
                Notes = orderDto.Notes,
                Status = OrderStatus.Pending,
                TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity),
                OrderDetails = new List<OrderDetail>()
            };

            // Tạo chi tiết đơn hàng cho từng sản phẩm trong giỏ
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Size = item.Size // Thêm thông tin size nếu có
                };
                order.OrderDetails.Add(orderDetail);

                // Trừ số lượng tồn kho của sản phẩm
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            await _cartService.ClearCartAsync(userId);

            // Lấy lại đơn hàng vừa tạo, bao gồm thông tin user và chi tiết đơn hàng
            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return MapToOrderDTO(createdOrder);
        }

        public async Task<OrderDTO?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO updateOrderStatusDto)
        {
            // Lấy đơn hàng theo Id
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }

            // Nếu hủy đơn hàng, hoàn lại số lượng sản phẩm về kho
            if (updateOrderStatusDto.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                        _context.Products.Update(product);
                    }
                }
            }

            order.Status = updateOrderStatusDto.Status;
            await _context.SaveChangesAsync();

            return MapToOrderDTO(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            // Lấy đơn hàng theo Id, bao gồm chi tiết đơn hàng
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return false;
            }

            // Nếu xóa đơn hàng chưa bị hủy, hoàn lại số lượng sản phẩm về kho
            if (order.Status != OrderStatus.Cancelled)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                        _context.Products.Update(product);
                    }
                }
            }

            // Xóa chi tiết đơn hàng và đơn hàng
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }

        private OrderDTO MapToOrderDTO(Order order)
        {
            // Chuyển đổi đối tượng Order sang OrderDTO để trả về cho client
            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.FullName,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                Status = order.Status,
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
                {
                    Id = od.Id,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name ?? "Unknown Product",
                    Price = od.Price,
                    Quantity = od.Quantity,
                    Size = od.Size, // Thêm thông tin size nếu có
                    ProductImageUrl = od.Product?.ImageUrl
                }).ToList() ?? new List<OrderDetailDTO>()
            };
        }
    }
}
