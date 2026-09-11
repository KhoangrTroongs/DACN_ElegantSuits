using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElegantSuits.Web.Models;

public class ResponseDTO<T>
{
    public bool IsSuccess { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public static ResponseDTO<T> Success(T data, string message = "Success") =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static ResponseDTO<T> Fail(string message, List<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

[JsonConverter(typeof(PaginatedListJsonConverterFactory))]
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PaginatedList()
    {
    }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = count;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;
        AddRange(items);
    }
}

public class PaginatedListJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;
        return typeToConvert.GetGenericTypeDefinition() == typeof(PaginatedList<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PaginatedListJsonConverter<>).MakeGenericType(itemType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

public class PaginatedListJsonConverter<T> : JsonConverter<PaginatedList<T>>
{
    public override PaginatedList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
            return new PaginatedList<T>(list, list.Count, 1, Math.Max(1, list.Count));
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject or StartArray for PaginatedList.");
        }

        var items = new List<T>();
        int pageIndex = 1;
        int pageSize = 10;
        int totalItems = 0;
        int totalPages = 0;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        foreach (var prop in root.EnumerateObject())
        {
            if (prop.NameEquals("items") || prop.NameEquals("Items"))
            {
                items = JsonSerializer.Deserialize<List<T>>(prop.Value.GetRawText(), options) ?? new List<T>();
            }
            else if (prop.NameEquals("pageIndex") || prop.NameEquals("PageIndex"))
            {
                pageIndex = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("pageSize") || prop.NameEquals("PageSize"))
            {
                pageSize = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("totalItems") || prop.NameEquals("TotalItems"))
            {
                totalItems = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("totalPages") || prop.NameEquals("TotalPages"))
            {
                totalPages = prop.Value.GetInt32();
            }
        }

        var result = new PaginatedList<T>(items, totalItems > 0 ? totalItems : items.Count, pageIndex, pageSize);
        if (totalPages > 0) result.TotalPages = totalPages;
        return result;
    }

    public override void Write(Utf8JsonWriter writer, PaginatedList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("PageIndex", value.PageIndex);
        writer.WriteNumber("PageSize", value.PageSize);
        writer.WriteNumber("TotalItems", value.TotalItems);
        writer.WriteNumber("TotalPages", value.TotalPages);
        writer.WriteBoolean("HasPreviousPage", value.HasPreviousPage);
        writer.WriteBoolean("HasNextPage", value.HasNextPage);
        writer.WritePropertyName("Items");
        JsonSerializer.Serialize(writer, (List<T>)value, options);
        writer.WriteEndObject();
    }
}

// ---------------- Product ViewModels ----------------
public class ProductViewModel
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

    public IFormFile? Image { get; set; }
    public string? ExistingImageUrl { get; set; }
    public IFormFile? Model3D { get; set; }
    public string? ExistingModel3DUrl { get; set; }
    public List<int> SelectedFabricIds { get; set; } = new List<int>();
    public IEnumerable<ElegantSuits.Domain.Entities.Category> Categories { get; set; } = new List<ElegantSuits.Domain.Entities.Category>();
    public IEnumerable<FabricDTO> Fabrics { get; set; } = new List<FabricDTO>();
}

public class CreateProductViewModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public bool IsHidden { get; set; }
    public string? Model3DUrl { get; set; }
    public decimal ProfitMargin { get; set; } = 0.45m;
    public string? LinearCode { get; set; }
    public IFormFile? Image { get; set; }
    public IFormFile? Model3D { get; set; }
}

public class UpdateProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public bool IsHidden { get; set; }
    public string? Model3DUrl { get; set; }
    public string? ImageUrl { get; set; }
    public decimal ProfitMargin { get; set; } = 0.45m;
    public string? LinearCode { get; set; }
    public IFormFile? Image { get; set; }
    public IFormFile? Model3D { get; set; }
}

// ---------------- Category ViewModels ----------------
public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryViewModel> SubCategories { get; set; } = new();
}

// ---------------- Cart ViewModels ----------------
public class CartItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public decimal TotalPrice => Price * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(x => x.TotalPrice);
    public decimal Discount { get; set; } = 0;
    public decimal TotalPrice => Math.Max(0, SubTotal - Discount);
    public string? AppliedCouponCode { get; set; }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Size { get; set; }
}

// ---------------- Order ViewModels ----------------
public class OrderDetailViewModel
{
    private decimal _unitPrice;
    private string? _imageUrl;

    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ProductImageUrl { get; set; }
    public string? ImageUrl
    {
        get => !string.IsNullOrEmpty(_imageUrl) ? _imageUrl : ProductImageUrl;
        set => _imageUrl = value;
    }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal UnitPrice
    {
        get => _unitPrice > 0 ? _unitPrice : Price;
        set => _unitPrice = value;
    }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? Size { get; set; }
}

public class OrderViewModel
{
    private string _orderStatus = "Pending";
    private List<OrderDetailViewModel> _details = new();

    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? UserName { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Status { get; set; }
    public string OrderStatus
    {
        get => !string.IsNullOrEmpty(Status) ? Status : _orderStatus;
        set => _orderStatus = value;
    }
    public string PaymentStatus { get; set; } = "Pending";
    public string PaymentMethod { get; set; } = "COD";
    public List<OrderDetailViewModel> OrderDetails { get; set; } = new();
    public List<OrderDetailViewModel> Details
    {
        get => _details.Any() ? _details : OrderDetails;
        set => _details = value;
    }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public string PaymentMethod { get; set; } = "COD";
    public string? CouponCode { get; set; }
}

// ---------------- Auth ViewModels ----------------
public class LoginViewModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    public int Gender { get; set; } = 0;
    public IFormFile? AvatarFile { get; set; }
}

public class AuthResponseViewModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public List<string>? Roles { get; set; }
}

public class ExternalLoginRequest
{
    public string Provider { get; set; } = "";
    public string ProviderKey { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FullName { get; set; }
}

// ---------------- Coupon ViewModels ----------------
// CouponViewModel maps ValidateCouponResult from BE
public class CouponViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    // From ValidateCouponResult
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
    public int DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; } // Pre-computed discount amount from BE
    // Aliases for view compatibility
    public decimal DiscountPercent => DiscountPercentage;
    public decimal MaxDiscountAmount { get; set; } = 0;
    public decimal MinOrderAmount { get; set; } = 0;
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

// ---------------- Fabric ViewModels ----------------
public class FabricViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Composition { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Pattern { get; set; }
    public decimal Price { get; set; }
    public decimal PricePerMeter { get; set; }
    public string? ImageUrl { get; set; }
    public int FabricGroupId { get; set; }
    public string? FabricGroupName { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class FabricGroupViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<FabricViewModel> Fabrics { get; set; } = new();
}
