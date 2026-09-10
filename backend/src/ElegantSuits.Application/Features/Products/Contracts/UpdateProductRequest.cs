namespace ElegantSuits.Application.Features.Products.Contracts;

public class UpdateProductRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public string? Model3DUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsHidden { get; set; } = false;
    public decimal ProfitMargin { get; set; } = 0.45m;
    public string? LinearCode { get; set; }
}
