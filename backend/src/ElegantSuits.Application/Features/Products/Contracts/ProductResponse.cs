namespace ElegantSuits.Application.Features.Products.Contracts;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public string? Model3DUrl { get; set; }
    public int Quantity { get; set; }
    public bool IsHidden { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? LinearCode { get; set; }
    public decimal ProfitMargin { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
