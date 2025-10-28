# 🛠️ HƯỚNG DẪN TRIỂN KHAI HỆ THỐNG COUPON

## 1. CẬP NHẬT DATABASE

### Thêm Cột Vào Bảng Coupon
```sql
ALTER TABLE Coupons ADD COLUMN MinimumAmount DECIMAL(18,2) DEFAULT 0;
ALTER TABLE Coupons ADD COLUMN MaxDiscountPercent DECIMAL(5,2) DEFAULT 25;
```

### Dữ Liệu Mẫu
```sql
INSERT INTO Coupons (Code, Quantity, DiscountPercentage, ExpiryDate, IsActive, MinimumAmount, MaxDiscountPercent, CreatedAt)
VALUES 
  ('COUPON5', -1, 5, '2025-12-31', 1, 300000, 5, GETDATE()),
  ('COUPON10', -1, 10, '2025-12-31', 1, 700000, 10, GETDATE()),
  ('COUPON15', -1, 15, '2025-12-31', 1, 1000000, 15, GETDATE()),
  ('COUPON20', -1, 20, '2025-12-31', 1, 1500000, 20, GETDATE()),
  ('COUPON25', -1, 25, '2025-12-31', 1, 2000000, 25, GETDATE());
```

---

## 2. CẬP NHẬT MODEL

### Coupon Model
```csharp
public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Thêm các cột mới
    public decimal MinimumAmount { get; set; } = 0;
    public decimal MaxDiscountPercent { get; set; } = 25;
}
```

### Product Model (Thêm Margin)
```csharp
public class Product
{
    // ... existing properties
    
    // Thêm cột margin
    public decimal ProfitMargin { get; set; } = 0.45m; // 45% mặc định
}
```

---

## 3. TẠO SERVICE LAYER

### ICouponService (Thêm Method)
```csharp
public interface ICouponService
{
    // Existing methods...
    
    // Thêm method mới
    Task<List<CouponDTO>> GetAvailableCouponsAsync(
        decimal cartTotal, 
        bool hasFreeship, 
        List<int> productIds);
    
    Task<CouponValidationResult> ValidateCouponForCheckoutAsync(
        string couponCode, 
        decimal cartTotal, 
        bool hasFreeship, 
        List<int> productIds);
}
```

### CouponService Implementation
```csharp
public async Task<List<CouponDTO>> GetAvailableCouponsAsync(
    decimal cartTotal, 
    bool hasFreeship, 
    List<int> productIds)
{
    var availableCoupons = new List<CouponDTO>();
    
    // Lấy tất cả coupon từ database
    var allCoupons = await _couponRepository.GetAllCouponsAsync();
    
    // Lấy thông tin sản phẩm
    var products = await _productRepository.GetProductsByIdsAsync(productIds);
    
    // Kiểm tra margin sản phẩm
    if (products.Any(p => p.ProfitMargin < 0.30m))
    {
        return availableCoupons; // Trả về danh sách trống
    }
    
    foreach (var coupon in allCoupons)
    {
        // Bước 1: Kiểm tra cơ bản
        if (coupon.ExpiryDate <= DateTime.Now || 
            coupon.Quantity == 0 || 
            !coupon.IsActive ||
            cartTotal < coupon.MinimumAmount)
        {
            continue;
        }
        
        // Bước 3: Tính lợi nhuận sau giảm
        var profitAfter = (cartTotal * 0.45m) - (cartTotal * (coupon.DiscountPercentage / 100));
        var profitRatio = profitAfter / cartTotal;
        
        if (profitRatio < 0.20m)
        {
            continue;
        }
        
        // Bước 4: Xử lý freeship
        if (hasFreeship && coupon.DiscountPercentage > 15)
        {
            continue;
        }
        
        availableCoupons.Add(MapToCouponDTO(coupon));
    }
    
    // Bước 5: Chọn coupon tốt nhất (% cao nhất)
    if (availableCoupons.Any())
    {
        var bestCoupon = availableCoupons.OrderByDescending(c => c.DiscountPercentage).First();
        return new List<CouponDTO> { bestCoupon };
    }
    
    return availableCoupons;
}
```

---

## 4. TẠO API ENDPOINT

### CouponController
```csharp
[HttpPost("available")]
public async Task<IActionResult> GetAvailableCoupons(
    [FromBody] AvailableCouponsRequest request)
{
    try
    {
        var availableCoupons = await _couponService.GetAvailableCouponsAsync(
            request.CartTotal,
            request.HasFreeship,
            request.ProductIds);
        
        return Ok(new
        {
            success = true,
            coupons = availableCoupons,
            message = availableCoupons.Any() ? 
                "Có mã giảm giá khả dụng" : 
                "Không có mã giảm giá khả dụng"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting available coupons");
        return Ok(new
        {
            success = false,
            message = "Lỗi khi lấy danh sách mã giảm giá"
        });
    }
}
```

### Request Model
```csharp
public class AvailableCouponsRequest
{
    public decimal CartTotal { get; set; }
    public bool HasFreeship { get; set; }
    public List<int> ProductIds { get; set; }
}
```

---

## 5. CẬP NHẬT CHECKOUT VIEW

### Checkout.cshtml
```html
<div class="coupon-section">
    <h5>🏷️ Mã Giảm Giá Khả Dụng</h5>
    <div id="availableCoupons" class="coupon-list">
        <!-- Danh sách coupon sẽ được load bằng AJAX -->
    </div>
    <div id="couponMessage" class="alert alert-info" style="display:none;"></div>
</div>

<script>
$(document).ready(function() {
    // Load available coupons khi trang load
    loadAvailableCoupons();
    
    // Reload khi giỏ hàng thay đổi
    $(document).on('cartUpdated', function() {
        loadAvailableCoupons();
    });
});

function loadAvailableCoupons() {
    var cartTotal = parseFloat($('#cartTotal').val());
    var hasFreeship = $('#hasFreeship').is(':checked');
    var productIds = getProductIds(); // Lấy danh sách product ID
    
    $.ajax({
        url: '@Url.Action("GetAvailableCoupons", "Coupon")',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            cartTotal: cartTotal,
            hasFreeship: hasFreeship,
            productIds: productIds
        }),
        success: function(response) {
            if (response.success && response.coupons.length > 0) {
                displayCoupons(response.coupons);
            } else {
                displayNoAvailableCoupons(cartTotal);
            }
        }
    });
}

function displayCoupons(coupons) {
    var html = '';
    coupons.forEach(function(coupon) {
        html += `
            <div class="coupon-item">
                <input type="radio" name="selectedCoupon" value="${coupon.code}">
                <label>
                    ${coupon.code} – Giảm ${coupon.discountPercentage}%
                    <small>(Áp dụng từ ${formatCurrency(coupon.minimumAmount)})</small>
                </label>
            </div>
        `;
    });
    $('#availableCoupons').html(html);
}

function displayNoAvailableCoupons(cartTotal) {
    var message = 'Mua thêm để được giảm giá';
    $('#couponMessage').text(message).show();
}
</script>
```

---

## 6. MIGRATION

### Tạo Migration
```bash
dotnet ef migrations add AddCouponMinimumAmountAndMargin
dotnet ef database update
```

---

## 7. TESTING

### Unit Test
```csharp
[TestClass]
public class CouponServiceTests
{
    [TestMethod]
    public async Task GetAvailableCoupons_WithValidCart_ReturnsBestCoupon()
    {
        // Arrange
        var cartTotal = 1200000m;
        var hasFreeship = false;
        var productIds = new List<int> { 1, 2 };
        
        // Act
        var result = await _couponService.GetAvailableCouponsAsync(
            cartTotal, hasFreeship, productIds);
        
        // Assert
        Assert.IsTrue(result.Any());
        Assert.AreEqual("COUPON15", result.First().Code);
    }
    
    [TestMethod]
    public async Task GetAvailableCoupons_WithFreeship_LimitTo15Percent()
    {
        // Arrange
        var cartTotal = 1800000m;
        var hasFreeship = true;
        var productIds = new List<int> { 1, 2 };
        
        // Act
        var result = await _couponService.GetAvailableCouponsAsync(
            cartTotal, hasFreeship, productIds);
        
        // Assert
        Assert.IsTrue(result.Any());
        Assert.IsTrue(result.First().DiscountPercentage <= 15);
    }
}
```

---

## 8. DEPLOYMENT CHECKLIST

- [ ] Cập nhật database schema
- [ ] Thêm dữ liệu coupon mẫu
- [ ] Cập nhật Model
- [ ] Implement Service Layer
- [ ] Tạo API Endpoint
- [ ] Cập nhật Checkout View
- [ ] Viết Unit Tests
- [ ] Test trên staging
- [ ] Deploy lên production
- [ ] Monitor logs

---

## 9. MONITORING

### Metrics Cần Theo Dõi
- Số lần coupon được hiển thị
- Số lần coupon được sử dụng
- Tỷ lệ conversion với coupon
- Lợi nhuận trung bình sau coupon
- Lỗi trong quá trình tính toán coupon

### Logging
```csharp
_logger.LogInformation(
    "Available coupons for cart {CartTotal}: {Coupons}",
    cartTotal,
    string.Join(", ", availableCoupons.Select(c => c.Code)));
```

