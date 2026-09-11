using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public static class SessionCartService
{
    private const string CartSessionKey = "SessionCart";

    public static CartViewModel GetSessionCart(ISession session)
    {
        var json = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new CartViewModel();
        }

        try
        {
            return JsonSerializer.Deserialize<CartViewModel>(json) ?? new CartViewModel();
        }
        catch
        {
            return new CartViewModel();
        }
    }

    public static void SaveSessionCart(ISession session, CartViewModel cart)
    {
        var json = JsonSerializer.Serialize(cart);
        session.SetString(CartSessionKey, json);
    }

    public static void AddToCart(ISession session, ProductViewModel product, int quantity, string? size)
    {
        var cart = GetSessionCart(session);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == product.Id && (i.Size ?? "") == (size ?? ""));

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItemViewModel
            {
                Id = cart.Items.Any() ? cart.Items.Max(x => x.Id) + 1 : 1,
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl ?? "",
                Price = product.Price,
                Quantity = quantity,
                Size = size
            });
        }

        SaveSessionCart(session, cart);
    }

    public static void UpdateQuantity(ISession session, int itemId, int quantity)
    {
        var cart = GetSessionCart(session);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            SaveSessionCart(session, cart);
        }
    }

    public static void RemoveItem(ISession session, int itemId)
    {
        var cart = GetSessionCart(session);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            cart.Items.Remove(item);
            SaveSessionCart(session, cart);
        }
    }

    public static void Clear(ISession session)
    {
        session.Remove(CartSessionKey);
    }

    public static int GetCount(ISession session)
    {
        var cart = GetSessionCart(session);
        return cart.Items.Sum(i => i.Quantity);
    }

    public static async Task SyncSessionCartToApiAsync(ISession session, ICartApiClient cartApiClient, string token)
    {
        var cart = GetSessionCart(session);
        if (cart.Items.Any())
        {
            foreach (var item in cart.Items)
            {
                await cartApiClient.AddToCartAsync(new AddToCartRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Size = item.Size
                }, token);
            }
            Clear(session);
        }
    }
}
