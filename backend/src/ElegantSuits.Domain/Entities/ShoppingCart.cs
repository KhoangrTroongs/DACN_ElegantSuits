namespace ElegantSuits.Domain.Entities;

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();

    public decimal GetTotal()
    {
        return Items.Sum(item => item.Price * item.Quantity);
    }
}
