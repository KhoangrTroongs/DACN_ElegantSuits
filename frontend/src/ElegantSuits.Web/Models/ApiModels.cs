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
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? Size { get; set; }
}

public class OrderViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderStatus { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Pending";
    public string PaymentMethod { get; set; } = "COD";
    public List<OrderDetailViewModel> Details { get; set; } = new();
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
