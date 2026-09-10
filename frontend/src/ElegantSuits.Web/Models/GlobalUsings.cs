namespace ElegantSuits.Web.Models;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public bool IsHidden { get; set; }
    public int CategoryId { get; set; }
    public int Quantity { get; set; }
}

public class ProductSizeViewModel
{
    public string Size { get; set; } = "";
    public int Quantity { get; set; }
}

public class ManageProductSizesViewModel
{
    public ElegantSuits.Domain.Entities.Product Product { get; set; } = new();
    public List<ElegantSuits.Domain.Entities.ProductSize> ProductSizes { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
    public List<ProductSizeViewModel> Sizes { get; set; } = new();
}
