# 🎯 PHÂN TÍCH HOÀN CHỈNH HỆ THỐNG COUPON

## 📌 TÓNG HỢP YÊU CẦU

Bạn muốn xây dựng hệ thống coupon tự động hiển thị coupon phù hợp dựa trên:
1. **Tổng giá trị hóa đơn** (Minimum Amount)
2. **Biên lợi nhuận sản phẩm** (≥ 30%)
3. **Trạng thái freeship** (Giới hạn ≤ 15%)
4. **Lợi nhuận hóa đơn** (≥ 20% sau giảm)

---

## 📊 COUPON HIỆN CÓ

```
COUPON5:  5% giảm | Tối thiểu 300.000đ
COUPON10: 10% giảm | Tối thiểu 700.000đ
COUPON15: 15% giảm | Tối thiểu 1.000.000đ
COUPON20: 20% giảm | Tối thiểu 1.500.000đ
COUPON25: 25% giảm | Tối thiểu 2.000.000đ
```

---

## ✅ LOGIC KIỂM TRA (5 BƯỚC)

### Bước 1: Lọc Coupon Cơ Bản
```
✓ ExpiryDate > DateTime.Now (chưa hết hạn)
✓ Quantity > 0 hoặc Quantity = -1 (còn lượt dùng)
✓ IsActive = true (đang kích hoạt)
✓ CartTotal >= MinimumAmount (đủ tiền)
```

### Bước 2: Kiểm Tra Margin Sản Phẩm
```
Điều kiện: Tất cả sản phẩm phải có Margin >= 30%

Nếu có sản phẩm Margin < 30%:
  → Loại bỏ TẤT CẢ coupon
  → Hiển thị: "Sản phẩm này không áp dụng giảm giá"
```

### Bước 3: Tính Lợi Nhuận Sau Giảm
```
Công thức:
  ProfitAfter = (CartTotal × 0.45) - (CartTotal × CouponDiscount%)
  ProfitRatio = ProfitAfter / CartTotal

Điều kiện: ProfitRatio >= 0.20 (20%)

Nếu ProfitRatio < 0.20:
  → Loại bỏ coupon đó
```

### Bước 4: Xử Lý Freeship
```
Nếu HasFreeship = true:
  Nếu CouponDiscount% > 15%:
    → Loại bỏ coupon đó
```

### Bước 5: Chọn Coupon Tốt Nhất
```
Từ danh sách coupon hợp lệ:
  → Chọn coupon có CouponDiscount% cao nhất
  → Chỉ hiển thị 1 coupon duy nhất
```

---

## 📈 BẢNG QUYẾT ĐỊNH NHANH

### Không Freeship (Giảm tối đa 25%)

| Tổng Tiền | Coupon | Giảm | Lợi Nhuận Sau | Hợp Lệ |
|-----------|--------|------|---------------|--------|
| 300K - 700K | COUPON5 | 5% | 40% | ✓ |
| 700K - 1M | COUPON10 | 10% | 35% | ✓ |
| 1M - 1.5M | COUPON15 | 15% | 30% | ✓ |
| 1.5M - 2M | COUPON20 | 20% | 25% | ✓ |
| ≥ 2M | COUPON25 | 25% | 20% | ✓ |

### Có Freeship (Giảm tối đa 15%)

| Tổng Tiền | Coupon | Giảm | Lợi Nhuận Sau | Hợp Lệ |
|-----------|--------|------|---------------|--------|
| 300K - 700K | COUPON5 | 5% | 40% | ✓ |
| 700K - 1M | COUPON10 | 10% | 35% | ✓ |
| 1M - 1.5M | COUPON15 | 15% | 30% | ✓ |
| 1.5M - 2M | Không | - | - | ✗ |
| ≥ 2M | Không | - | - | ✗ |

---

## 💬 THÔNG BÁO HIỂN THỊ

### Khi Có Coupon Khả Dụng
```
🏷️ Giảm giá khả dụng cho bạn:
   COUPON15 – Giảm 15%
   Tiết kiệm: 180.000đ
   (Áp dụng cho đơn từ 1.000.000đ)
```

### Khi Chưa Đủ Tiền
```
💰 Mua thêm 200.000đ để được giảm 10% (COUPON10)
💰 Mua thêm 800.000đ để được giảm 15% (COUPON15)
💰 Mua thêm 1.300.000đ để được giảm 20% (COUPON20)
```

### Khi Margin Không Đủ
```
❌ Sản phẩm này không áp dụng mã giảm giá
   (Biên lợi nhuận không đủ)
```

### Khi Có Freeship
```
⚠️ Với đơn hàng freeship, giảm giá tối đa 15%
🏷️ Giảm giá khả dụng:
   COUPON15 – Giảm 15%
   Tiết kiệm: 225.000đ
```

---

## 🔧 CÔNG THỨC TÍNH TOÁN

### Ví Dụ: Đơn 1.200.000đ (Không Freeship)

**COUPON5 (5%)**
```
Tiền giảm = 1.200.000 × 5% = 60.000đ
Lợi nhuận gốc = 1.200.000 × 45% = 540.000đ
Lợi nhuận sau = 540.000 - 60.000 = 480.000đ
Tỷ lệ = 480.000 / 1.200.000 = 40% ✓
```

**COUPON10 (10%)**
```
Tiền giảm = 1.200.000 × 10% = 120.000đ
Lợi nhuận gốc = 1.200.000 × 45% = 540.000đ
Lợi nhuận sau = 540.000 - 120.000 = 420.000đ
Tỷ lệ = 420.000 / 1.200.000 = 35% ✓
```

**COUPON15 (15%)**
```
Tiền giảm = 1.200.000 × 15% = 180.000đ
Lợi nhuận gốc = 1.200.000 × 45% = 540.000đ
Lợi nhuận sau = 540.000 - 180.000 = 360.000đ
Tỷ lệ = 360.000 / 1.200.000 = 30% ✓
```

**Kết luận**: Hiển thị **COUPON15** (cao nhất)

---

## 🎯 PSEUDOCODE

```
FUNCTION GetAvailableCoupons(cartTotal, hasFreeship, products):
  
  // Kiểm tra margin sản phẩm
  IF ANY product.margin < 0.30:
    RETURN [] // Không có coupon
  
  availableCoupons = []
  
  FOR EACH coupon IN database:
    // Bước 1: Lọc cơ bản
    IF coupon.expiryDate <= NOW OR coupon.quantity == 0 OR !coupon.isActive:
      CONTINUE
    IF cartTotal < coupon.minimumAmount:
      CONTINUE
    
    // Bước 3: Tính lợi nhuận
    profitAfter = (cartTotal × 0.45) - (cartTotal × coupon.discount% / 100)
    profitRatio = profitAfter / cartTotal
    
    IF profitRatio < 0.20:
      CONTINUE
    
    // Bước 4: Xử lý freeship
    IF hasFreeship AND coupon.discount% > 15:
      CONTINUE
    
    availableCoupons.ADD(coupon)
  
  // Bước 5: Chọn tốt nhất
  IF availableCoupons.COUNT > 0:
    RETURN [availableCoupons.MAX(discount%)]
  ELSE:
    RETURN []
```

---

## 📋 BẢNG TÍNH TOÁN ĐẦY ĐỦ (1.500.000đ)

| Coupon | Giảm | Tiền Giảm | Lợi Nhuận Gốc | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|---------------|---------------|-------|--------|
| COUPON5 | 5% | 75.000đ | 675.000đ | 600.000đ | 40% | ✓ |
| COUPON10 | 10% | 150.000đ | 675.000đ | 525.000đ | 35% | ✓ |
| COUPON15 | 15% | 225.000đ | 675.000đ | 450.000đ | 30% | ✓ |
| COUPON20 | 20% | 300.000đ | 675.000đ | 375.000đ | 25% | ✓ |
| COUPON25 | 25% | 375.000đ | 675.000đ | 300.000đ | 20% | ✓ |

**Kết luận**: Hiển thị **COUPON25** (cao nhất, vẫn đạt 20%)

---

## 🚀 TRIỂN KHAI

### 1. Database
```sql
ALTER TABLE Coupons ADD MinimumAmount DECIMAL(18,2) DEFAULT 0;
ALTER TABLE Products ADD ProfitMargin DECIMAL(5,2) DEFAULT 0.45;
```

### 2. Service Layer
```csharp
Task<List<CouponDTO>> GetAvailableCouponsAsync(
    decimal cartTotal, 
    bool hasFreeship, 
    List<int> productIds);
```

### 3. API Endpoint
```
POST /Coupon/available
Body: { cartTotal, hasFreeship, productIds }
Response: { coupons: [...], message: "..." }
```

### 4. UI/UX
- AJAX load coupon khi giỏ hàng thay đổi
- Hiển thị 1 coupon tốt nhất
- Thông báo gợi ý nếu chưa đủ tiền

---

## 📊 MONITORING

- Số lần coupon được hiển thị
- Số lần coupon được sử dụng
- Tỷ lệ conversion
- Lợi nhuận trung bình sau coupon
- Lỗi trong quá trình tính toán

---

## ✨ TÍNH NĂNG NÂNG CAO (Tương Lai)

- [ ] Coupon theo danh mục sản phẩm
- [ ] Coupon theo khách hàng VIP
- [ ] Coupon theo thời gian (flash sale)
- [ ] Coupon kết hợp (stack multiple)
- [ ] Coupon referral
- [ ] A/B testing coupon

---

## 📚 TÀI LIỆU THAM KHẢO

1. **COUPON_LOGIC_ANALYSIS.md** - Phân tích chi tiết
2. **COUPON_CALCULATION_DETAILS.md** - Bảng tính toán
3. **COUPON_REAL_EXAMPLES.md** - Ví dụ thực tế
4. **COUPON_IMPLEMENTATION_GUIDE.md** - Hướng dẫn code

