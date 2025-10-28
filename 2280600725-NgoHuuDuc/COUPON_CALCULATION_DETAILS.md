# 🧮 CHI TIẾT TÍNH TOÁN COUPON

## 1. BẢNG TÍNH TOÁN ĐẦY ĐỦ (Không Freeship)

### Scenario: Đơn hàng 1.500.000đ

| Coupon | Giảm | Tiền Giảm | Tổng Sau | Lợi Nhuận Gốc | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|----------|---------------|---------------|-------|--------|
| COUPON5 | 5% | 75.000đ | 1.425.000đ | 675.000đ | 600.000đ | 40% | ✓ |
| COUPON10 | 10% | 150.000đ | 1.350.000đ | 675.000đ | 525.000đ | 35% | ✓ |
| COUPON15 | 15% | 225.000đ | 1.275.000đ | 675.000đ | 450.000đ | 30% | ✓ |
| COUPON20 | 20% | 300.000đ | 1.200.000đ | 675.000đ | 375.000đ | 25% | ✓ |
| COUPON25 | 25% | 375.000đ | 1.125.000đ | 675.000đ | 300.000đ | 20% | ✓ |

**Kết luận**: Hiển thị **COUPON25** (cao nhất, vẫn đạt 20%)

---

## 2. BẢNG TÍNH TOÁN ĐẦY ĐỦ (Có Freeship)

### Scenario: Đơn hàng 1.500.000đ + Freeship

| Coupon | Giảm | Tiền Giảm | Tổng Sau | Lợi Nhuận Gốc | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|----------|---------------|---------------|-------|--------|
| COUPON5 | 5% | 75.000đ | 1.425.000đ | 675.000đ | 600.000đ | 40% | ✓ |
| COUPON10 | 10% | 150.000đ | 1.350.000đ | 675.000đ | 525.000đ | 35% | ✓ |
| COUPON15 | 15% | 225.000đ | 1.275.000đ | 675.000đ | 450.000đ | 30% | ✓ |
| COUPON20 | 20% | 300.000đ | 1.200.000đ | 675.000đ | 375.000đ | 25% | ✗ (Vượt giới hạn) |
| COUPON25 | 25% | 375.000đ | 1.125.000đ | 675.000đ | 300.000đ | 20% | ✗ (Vượt giới hạn) |

**Kết luận**: Hiển thị **COUPON15** (cao nhất trong giới hạn 15%)

---

## 3. BẢNG NGƯỠNG COUPON THEO TỔNG TIỀN

### Không Freeship

```
Tổng Tiền          | Coupon Khả Dụng | Giảm | Tiết Kiệm (VD)
300.000 - 699.999  | COUPON5         | 5%  | 15.000 - 35.000đ
700.000 - 999.999  | COUPON10        | 10% | 70.000 - 100.000đ
1.000.000 - 1.499.999 | COUPON15     | 15% | 150.000 - 225.000đ
1.500.000 - 1.999.999 | COUPON20     | 20% | 300.000 - 400.000đ
≥ 2.000.000        | COUPON25        | 25% | ≥ 500.000đ
```

### Có Freeship

```
Tổng Tiền          | Coupon Khả Dụng | Giảm | Tiết Kiệm (VD)
300.000 - 699.999  | COUPON5         | 5%  | 15.000 - 35.000đ
700.000 - 999.999  | COUPON10        | 10% | 70.000 - 100.000đ
1.000.000 - 1.499.999 | COUPON15     | 15% | 150.000 - 225.000đ
1.500.000 - 1.999.999 | Không có      | -   | -
≥ 2.000.000        | Không có        | -   | -
```

---

## 4. LOGIC KIỂM TRA TỪNG BƯỚC

### Bước 1: Kiểm Tra Coupon Cơ Bản
```
✓ ExpiryDate > DateTime.Now
✓ Quantity > 0 hoặc Quantity = -1
✓ IsActive = true
✓ CartTotal >= MinimumAmount
```

### Bước 2: Kiểm Tra Biên Lợi Nhuận Sản Phẩm
```
Điều kiện: Tất cả sản phẩm phải có Margin >= 30%

Nếu có sản phẩm Margin < 30%:
  → Loại bỏ tất cả coupon
  → Hiển thị thông báo: "Sản phẩm này không áp dụng giảm giá"
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
  → Chỉ hiển thị 1 coupon
```

---

## 5. THÔNG BÁO GỢI Ý CHI TIẾT

### Khi Đủ Điều Kiện
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

### Khi Có Freeship
```
⚠️ Với đơn hàng freeship, giảm giá tối đa 15%

🏷️ Giảm giá khả dụng:
   COUPON15 – Giảm 15%
   Tiết kiệm: 225.000đ
```

### Khi Sản Phẩm Không Đủ Điều Kiện
```
❌ Sản phẩm này không áp dụng mã giảm giá
   (Biên lợi nhuận không đủ)
```

---

## 6. BẢNG QUYẾT ĐỊNH NHANH

| Tổng Tiền | Freeship | Margin ≥30% | Coupon Hiển Thị | Ghi Chú |
|-----------|----------|-------------|-----------------|---------|
| 250.000đ | Không | ✓ | Không | Dưới 300.000đ |
| 500.000đ | Không | ✓ | COUPON5 (5%) | Tiết kiệm 25.000đ |
| 800.000đ | Không | ✓ | COUPON10 (10%) | Tiết kiệm 80.000đ |
| 1.200.000đ | Không | ✓ | COUPON15 (15%) | Tiết kiệm 180.000đ |
| 1.800.000đ | Không | ✓ | COUPON20 (20%) | Tiết kiệm 360.000đ |
| 2.500.000đ | Không | ✓ | COUPON25 (25%) | Tiết kiệm 625.000đ |
| 1.200.000đ | Có | ✓ | COUPON15 (15%) | Giới hạn 15% |
| 1.800.000đ | Có | ✓ | COUPON15 (15%) | Giới hạn 15% |
| 2.500.000đ | Có | ✓ | COUPON15 (15%) | Giới hạn 15% |
| 1.200.000đ | Không | ✗ | Không | Margin < 30% |

---

## 7. PSEUDOCODE LOGIC

```
FUNCTION GetAvailableCoupons(cartTotal, hasFreeship, products):
  availableCoupons = []
  
  FOR EACH coupon IN database:
    // Bước 1: Kiểm tra cơ bản
    IF coupon.ExpiryDate <= NOW OR coupon.Quantity == 0 OR !coupon.IsActive:
      CONTINUE
    
    IF cartTotal < coupon.MinimumAmount:
      CONTINUE
    
    // Bước 2: Kiểm tra margin sản phẩm
    IF ANY product.Margin < 0.30:
      BREAK (loại bỏ tất cả coupon)
    
    // Bước 3: Tính lợi nhuận sau giảm
    profitAfter = (cartTotal × 0.45) - (cartTotal × coupon.Discount%)
    profitRatio = profitAfter / cartTotal
    
    IF profitRatio < 0.20:
      CONTINUE
    
    // Bước 4: Xử lý freeship
    IF hasFreeship AND coupon.Discount% > 0.15:
      CONTINUE
    
    // Bước 5: Thêm vào danh sách
    availableCoupons.ADD(coupon)
  
  // Chọn coupon tốt nhất
  IF availableCoupons.COUNT > 0:
    RETURN availableCoupons.MAX(Discount%)
  ELSE:
    RETURN NULL
```

---

## 8. TÓNG HỢP QUYẾT ĐỊNH

**Coupon được hiển thị khi:**
1. ✓ Chưa hết hạn
2. ✓ Còn lượt dùng
3. ✓ Đang kích hoạt
4. ✓ Tổng tiền ≥ mức tối thiểu
5. ✓ Tất cả sản phẩm margin ≥ 30%
6. ✓ Lợi nhuận sau giảm ≥ 20%
7. ✓ Nếu freeship: giảm ≤ 15%
8. ✓ Là coupon có % cao nhất trong danh sách hợp lệ

