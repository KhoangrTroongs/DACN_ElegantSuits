using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ApplicationDbContext context,
            ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartDTO?> GetCartAsync(string userId)
        {
            // Lấy giỏ hàng của user kèm theo các sản phẩm trong giỏ (bao gồm cả thông tin sản phẩm)
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart != null ? MapToCartDTO(cart) : null;
        }

        public async Task<CartDTO> AddToCartAsync(string userId, AddToCartDTO addToCartDto)
        {
            // Kiểm tra sản phẩm có tồn tại và còn đủ số lượng không
            var product = await _context.Products.FindAsync(addToCartDto.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            if (product.Quantity < addToCartDto.Quantity)
            {
                throw new InvalidOperationException("Số lượng sản phẩm không đủ.");
            }

            // Lấy giỏ hàng hiện tại hoặc tạo mới nếu chưa có
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);
            if (cartItem != null)
            {
                // Nếu đã có thì cộng thêm số lượng
                cartItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                // Nếu chưa có thì thêm mới sản phẩm vào giỏ hàng
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = addToCartDto.Quantity,
                    ImageUrl = product.ImageUrl
                };
                // Thêm sản phẩm mới vào giỏ hàng nếu chưa có
                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Sau khi cập nhật giỏ hàng, load lại giỏ hàng kèm thông tin sản phẩm để trả về cho client
            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            return MapToCartDTO(cart);
        }

        public async Task<CartDTO?> UpdateCartItemAsync(string userId, UpdateCartItemDTO updateCartItemDto)
        {
            // Lấy giỏ hàng của user kèm các sản phẩm
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return null;
            }

            // Tìm sản phẩm trong giỏ hàng theo CartItemId
            var cartItem = cart.Items.FirstOrDefault(i => i.Id == updateCartItemDto.CartItemId);
            if (cartItem == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại trong giỏ hàng.");
            }

            // Kiểm tra lại số lượng tồn kho của sản phẩm
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            if (product.Quantity < updateCartItemDto.Quantity)
            {
                throw new InvalidOperationException("Số lượng sản phẩm không đủ.");
            }

            if (updateCartItemDto.Quantity <= 0)
            {
                // Nếu số lượng <= 0 thì xóa sản phẩm khỏi giỏ hàng
                cart.Items.Remove(cartItem);
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Nếu số lượng > 0 thì cập nhật lại số lượng
                cartItem.Quantity = updateCartItemDto.Quantity;
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return MapToCartDTO(cart);
        }

        public async Task<bool> RemoveCartItemAsync(string userId, int cartItemId)
        {
            // Lấy giỏ hàng của user
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return false;
            }

            // Tìm sản phẩm trong giỏ hàng theo Id
            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
            {
                return false;
            }

            // Xóa sản phẩm khỏi giỏ hàng
            cart.Items.Remove(cartItem);
            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            // Lấy giỏ hàng của user
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return false;
            }

            // Xóa toàn bộ sản phẩm trong giỏ hàng
            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        private CartDTO MapToCartDTO(Cart cart)
        {
            // Chuyển đổi đối tượng Cart sang CartDTO để trả về cho client.
            // Hàm này lấy thông tin từ Cart và các CartItem, chuyển sang dạng DTO phù hợp cho API trả về.
            return new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList(),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }
    }
}
