using ElegantSuits.Application.Features.Cart.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface ICartRepository
{
    Task<CartDTO?> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartDTO> AddToCartAsync(string userId, AddToCartDTO addToCartDto, CancellationToken cancellationToken = default);
    Task<CartDTO?> UpdateCartItemAsync(string userId, UpdateCartItemDTO updateCartItemDto, CancellationToken cancellationToken = default);
    Task<bool> RemoveCartItemAsync(string userId, int cartItemId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}
