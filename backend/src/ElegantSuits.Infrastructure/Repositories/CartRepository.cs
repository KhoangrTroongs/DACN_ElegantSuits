using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Cart.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDTO?> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        return cart != null ? MapToCartDTO(cart) : null;
    }

    public async Task<CartDTO> AddToCartAsync(string userId, AddToCartDTO addToCartDto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { addToCartDto.ProductId }, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException("Sản phẩm không tồn tại.");
        }

        if (product.Quantity < addToCartDto.Quantity)
        {
            throw new InvalidOperationException("Số lượng sản phẩm không đủ.");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId && i.Size == addToCartDto.Size);
        if (cartItem != null)
        {
            cartItem.Quantity += addToCartDto.Quantity;
        }
        else
        {
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = addToCartDto.Quantity,
                ImageUrl = product.ImageUrl,
                Size = addToCartDto.Size
            };
            cart.Items.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == cart.Id, cancellationToken);

        return MapToCartDTO(cart!);
    }

    public async Task<CartDTO?> UpdateCartItemAsync(string userId, UpdateCartItemDTO updateCartItemDto, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return null;

        var cartItem = cart.Items.FirstOrDefault(i => i.Id == updateCartItemDto.CartItemId);
        if (cartItem == null)
        {
            throw new InvalidOperationException("Sản phẩm không tồn tại trong giỏ hàng.");
        }

        var product = await _context.Products.FindAsync(new object[] { cartItem.ProductId }, cancellationToken);
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
            cart.Items.Remove(cartItem);
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = updateCartItemDto.Quantity;
        }

        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToCartDTO(cart);
    }

    public async Task<bool> RemoveCartItemAsync(string userId, int cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return false;

        var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (cartItem == null) return false;

        cart.Items.Remove(cartItem);
        _context.CartItems.Remove(cartItem);
        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return false;

        _context.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CartDTO MapToCartDTO(Cart cart)
    {
        return new CartDTO
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDTO
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = !string.IsNullOrEmpty(i.ProductName) ? i.ProductName : (i.Product != null ? i.Product.Name : ""),
                Price = i.Price > 0 ? i.Price : (i.Product != null ? i.Product.Price : 0),
                Quantity = i.Quantity,
                ImageUrl = !string.IsNullOrEmpty(i.ImageUrl) ? i.ImageUrl : (i.Product != null ? (i.Product.ImageUrl ?? "") : ""),
                Size = i.Size
            }).ToList(),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
