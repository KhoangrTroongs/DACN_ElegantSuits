namespace ElegantSuits.Application.Features.Products.Contracts;

public class CreateProductRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsHidden { get; set; } = false;
    public int CategoryId { get; set; }
    public string? Model3DUrl { get; set; }
    public decimal ProfitMargin { get; set; } = 0.45m;
    public string? LinearCode { get; set; }
}
