# Coupon System - Code Reference Guide

## Key Code Snippets

### 1. Coupon Model
```csharp
public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
```

### 2. Order Model Updates
```csharp
public class Order
{
    // ... existing properties ...
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
}
```

### 3. Coupon Validation Logic
```csharp
public async Task<CouponValidationResult> ValidateCouponAsync(string code)
{
    var coupon = await _couponRepository.GetCouponByCodeAsync(code);
    
    if (coupon == null)
        return new CouponValidationResult { 
            IsValid = false, 
            ErrorMessage = "Mã giảm giá không tồn tại" 
        };
    
    if (!coupon.IsActive)
        return new CouponValidationResult { 
            IsValid = false, 
            ErrorMessage = "Mã giảm giá không khả dụng" 
        };
    
    if (DateTime.Now > coupon.ExpiryDate)
        return new CouponValidationResult { 
            IsValid = false, 
            ErrorMessage = "Mã giảm giá đã hết hạn" 
        };
    
    if (coupon.Quantity == 0)
        return new CouponValidationResult { 
            IsValid = false, 
            ErrorMessage = "Mã giảm giá đã hết số lượng" 
        };
    
    return new CouponValidationResult { 
        IsValid = true, 
        Coupon = MapToCouponDTO(coupon) 
    };
}
```

### 4. Checkout with Coupon
```csharp
[HttpPost]
public async Task<IActionResult> Checkout(Order order)
{
    var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
    var discountAmount = 0m;
    var couponCode = order.CouponCode;

    if (!string.IsNullOrWhiteSpace(couponCode))
    {
        var validationResult = await _couponService.ValidateCouponAsync(couponCode);
        if (!validationResult.IsValid)
        {
            TempData["ErrorMessage"] = validationResult.ErrorMessage;
            return View(order);
        }
        discountAmount = (totalPrice * validationResult.Coupon.DiscountPercentage) / 100;
    }

    var newOrder = new Order
    {
        UserId = user.Id,
        OrderDate = DateTime.Now,
        TotalPrice = totalPrice - discountAmount,
        Status = OrderStatus.Pending,
        ShippingAddress = order.ShippingAddress,
        Notes = order.Notes,
        CouponCode = !string.IsNullOrWhiteSpace(couponCode) ? couponCode.ToUpper() : null,
        DiscountAmount = discountAmount
    };

    // ... save order ...

    if (!string.IsNullOrWhiteSpace(couponCode))
    {
        await _couponService.DecrementCouponQuantityAsync(couponCode);
    }
}
```

### 5. AJAX Coupon Validation
```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> ValidateCoupon(string couponCode)
{
    if (string.IsNullOrWhiteSpace(couponCode))
        return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });

    var validationResult = await _couponService.ValidateCouponAsync(couponCode);
    if (!validationResult.IsValid)
        return Json(new { success = false, message = validationResult.ErrorMessage });

    return Json(new 
    { 
        success = true, 
        discountPercentage = validationResult.Coupon.DiscountPercentage,
        message = $"Áp dụng mã giảm giá thành công! Giảm {validationResult.Coupon.DiscountPercentage}%"
    });
}
```

### 6. JavaScript Discount Calculation
```javascript
$('#applyCouponBtn').click(function () {
    const couponCode = $('#CouponCode').val().trim();
    
    $.ajax({
        url: '@Url.Action("ValidateCoupon", "ShoppingCart")',
        type: 'POST',
        data: { couponCode: couponCode },
        success: function (response) {
            if (response.success) {
                const discountAmount = (subtotal * response.discountPercentage) / 100;
                const total = subtotal - discountAmount;
                
                $('#discountRow').show();
                $('#discountAmount').text('-' + Math.round(discountAmount).toLocaleString('vi-VN') + 'đ');
                $('#totalAmount').text(Math.round(total).toLocaleString('vi-VN') + 'đ');
            }
        }
    });
});
```

### 7. Coupon Quantity Decrement
```csharp
public async Task<bool> DecrementCouponQuantityAsync(string code)
{
    var coupon = await _couponRepository.GetCouponByCodeAsync(code);
    if (coupon == null) return false;

    // Only decrement if quantity is not unlimited (-1)
    if (coupon.Quantity != -1)
    {
        coupon.Quantity--;
    }

    await _couponRepository.UpdateCouponAsync(coupon);
    return true;
}
```

### 8. Case-Insensitive Code Lookup
```csharp
public async Task<Coupon?> GetCouponByCodeAsync(string code)
{
    return await _context.Coupons
        .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());
}
```

### 9. Dependency Injection Registration
```csharp
// In Program.cs
builder.Services.AddScoped<ICouponRepository, EFCouponRepository>();
builder.Services.AddScoped<ICouponService, CouponService>();
```

### 10. DbContext Configuration
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // ... existing DbSets ...
    public DbSet<Coupon> Coupons { get; set; }
}
```

---

## Common Patterns Used

### Pattern 1: Repository Pattern
```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### Pattern 2: Service Layer Pattern
```csharp
public interface IService<TDto>
{
    Task<TDto> GetByIdAsync(int id);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto> AddAsync(CreateDto dto);
    Task<TDto> UpdateAsync(int id, UpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
```

### Pattern 3: DTO Mapping
```csharp
private CouponDTO MapToCouponDTO(Coupon coupon)
{
    return new CouponDTO
    {
        Id = coupon.Id,
        Code = coupon.Code,
        Quantity = coupon.Quantity,
        DiscountPercentage = coupon.DiscountPercentage,
        ExpiryDate = coupon.ExpiryDate,
        IsActive = coupon.IsActive,
        CreatedAt = coupon.CreatedAt,
        UpdatedAt = coupon.UpdatedAt
    };
}
```

### Pattern 4: Async/Await
```csharp
public async Task<IActionResult> Index()
{
    var coupons = await _couponService.GetAllCouponsAsync();
    return View(coupons);
}
```

### Pattern 5: Error Handling
```csharp
try
{
    // Business logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    TempData["ErrorMessage"] = "User-friendly error message";
    return RedirectToAction(nameof(Index));
}
```

---

## SQL Queries Generated

### Get Coupon by Code
```sql
SELECT * FROM Coupons 
WHERE UPPER(Code) = UPPER(@code)
```

### Get All Coupons
```sql
SELECT * FROM Coupons 
ORDER BY CreatedAt DESC
```

### Update Coupon Quantity
```sql
UPDATE Coupons 
SET Quantity = Quantity - 1, UpdatedAt = GETDATE()
WHERE UPPER(Code) = UPPER(@code) AND Quantity != -1
```

### Check Coupon Exists
```sql
SELECT COUNT(*) FROM Coupons 
WHERE Id = @id
```

---

## Configuration Files

### Program.cs Additions
```csharp
// Register repositories
builder.Services.AddScoped<ICouponRepository, EFCouponRepository>();

// Register services
builder.Services.AddScoped<ICouponService, CouponService>();
```

### ApplicationDbContext Additions
```csharp
public DbSet<Coupon> Coupons { get; set; }
```

---

## View Model Examples

### Create Coupon Form
```html
<form asp-action="Create" method="post">
    <input asp-for="Code" class="form-control" />
    <input asp-for="DiscountPercentage" type="number" step="0.01" />
    <input asp-for="Quantity" type="number" />
    <input asp-for="ExpiryDate" type="datetime-local" />
    <input asp-for="IsActive" type="checkbox" />
    <button type="submit">Create</button>
</form>
```

### Checkout Coupon Input
```html
<div class="input-group">
    <input type="text" id="CouponCode" name="CouponCode" 
           class="form-control" placeholder="Enter coupon code" />
    <button type="button" id="applyCouponBtn" class="btn btn-outline-secondary">
        Apply
    </button>
</div>
```

---

## Testing Examples

### Test Valid Coupon
```csharp
[Fact]
public async Task ValidateCoupon_WithValidCode_ReturnsSuccess()
{
    var coupon = new Coupon { Code = "TEST10", IsActive = true, 
                              Quantity = 10, ExpiryDate = DateTime.Now.AddDays(1) };
    var result = await _service.ValidateCouponAsync("TEST10");
    Assert.True(result.IsValid);
}
```

### Test Expired Coupon
```csharp
[Fact]
public async Task ValidateCoupon_WithExpiredDate_ReturnsFailed()
{
    var coupon = new Coupon { Code = "EXPIRED", ExpiryDate = DateTime.Now.AddDays(-1) };
    var result = await _service.ValidateCouponAsync("EXPIRED");
    Assert.False(result.IsValid);
    Assert.Contains("hết hạn", result.ErrorMessage);
}
```

---

**Reference Version**: 1.0
**Last Updated**: October 28, 2024

