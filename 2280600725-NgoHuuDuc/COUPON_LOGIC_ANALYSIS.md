# 📊 PHÂN TÍCH HỆ THỐNG LOGIC COUPON

## 1. BẢNG LOGIC ĐIỀU KIỆN ÁP DỤNG

| Coupon | Giảm | Tối Thiểu | Điều Kiện | Ghi Chú |
|--------|------|-----------|-----------|---------|
| COUPON5 | 5% | 300.000đ | Lợi nhuận ≥ 30% | Luôn khả dụng nếu đủ tiền |
| COUPON10 | 10% | 700.000đ | Lợi nhuận ≥ 30% | Ưu tiên hơn COUPON5 |
| COUPON15 | 15% | 1.000.000đ | Lợi nhuận ≥ 30% | Ưu tiên hơn COUPON10 |
| COUPON20 | 20% | 1.500.000đ | Lợi nhuận ≥ 30% | Ưu tiên hơn COUPON15 |
| COUPON25 | 25% | 2.000.000đ | Lợi nhuận ≥ 30% | Tối đa cho phép |

---

## 2. QUY TRÌNH KIỂM TRA COUPON

### Bước 1: Lọc Coupon Cơ Bản
```
✓ Coupon chưa hết hạn (ExpiryDate > Now)
✓ Coupon còn lượt dùng (Quantity > 0 hoặc Quantity = -1)
✓ Coupon đang kích hoạt (IsActive = true)
✓ Tổng tiền ≥ Mức tối thiểu
```

### Bước 2: Kiểm Tra Lợi Nhuận Sản Phẩm
```
Điều kiện: Tất cả sản phẩm trong giỏ phải có biên lợi nhuận ≥ 30%
Nếu có sản phẩm < 30% → Loại bỏ coupon đó
```

### Bước 3: Tính Toán Lợi Nhuận Sau Giảm
```
Công thức:
- Tổng tiền gốc = Σ(Giá sản phẩm)
- Tổng lợi nhuận gốc = Tổng tiền × 45% (biên lợi nhuận trung bình)
- Tiền giảm = Tổng tiền × Phần trăm coupon
- Lợi nhuận sau giảm = Tổng lợi nhuận gốc - Tiền giảm
- Tỷ lệ lợi nhuận sau = Lợi nhuận sau giảm / Tổng tiền

Điều kiện: Tỷ lệ lợi nhuận sau ≥ 20%
Nếu < 20% → Loại bỏ coupon đó
```

### Bước 4: Xử Lý Freeship
```
Nếu hóa đơn có freeship:
  - Giảm tối đa 15% (thay vì 25%)
  - Kiểm tra lại lợi nhuận với giới hạn 15%
```

### Bước 5: Chọn Coupon Tốt Nhất
```
Nếu nhiều coupon đủ điều kiện:
  - Chỉ hiển thị coupon có phần trăm cao nhất
  - Ưu tiên: COUPON25 > COUPON20 > COUPON15 > COUPON10 > COUPON5
```

---

## 3. BẢNG DANH SÁCH COUPON KHẢ DỤNG

### Theo Mức Tổng Hóa Đơn (Không Freeship)

| Tổng Tiền | Coupon Khả Dụng | Giảm | Lợi Nhuận Sau |
|-----------|-----------------|------|---------------|
| 300.000đ - 699.999đ | COUPON5 | 5% | 42.5% ✓ |
| 700.000đ - 999.999đ | COUPON10 | 10% | 40% ✓ |
| 1.000.000đ - 1.499.999đ | COUPON15 | 15% | 37.5% ✓ |
| 1.500.000đ - 1.999.999đ | COUPON20 | 20% | 35% ✓ |
| ≥ 2.000.000đ | COUPON25 | 25% | 32.5% ✓ |

### Theo Mức Tổng Hóa Đơn (Có Freeship)

| Tổng Tiền | Coupon Khả Dụng | Giảm | Lợi Nhuận Sau |
|-----------|-----------------|------|---------------|
| 300.000đ - 699.999đ | COUPON5 | 5% | 42.5% ✓ |
| 700.000đ - 999.999đ | COUPON10 | 10% | 40% ✓ |
| 1.000.000đ - 1.499.999đ | COUPON15 | 15% | 37.5% ✓ |
| 1.500.000đ - 1.999.999đ | Không có | - | - |
| ≥ 2.000.000đ | Không có | - | - |

**Lý do**: Với freeship, COUPON20 và COUPON25 sẽ làm lợi nhuận < 20%

---

## 4. LOGIC KIỂM TRA CHI TIẾT

### Ví Dụ 1: Đơn 1.200.000đ (Không Freeship)
```
Bước 1: Lọc cơ bản
  ✓ COUPON5, COUPON10, COUPON15 đủ điều kiện

Bước 2: Kiểm tra lợi nhuận sản phẩm
  ✓ Tất cả sản phẩm ≥ 30%

Bước 3: Tính lợi nhuận sau giảm
  - COUPON5: 1.200.000 × 5% = 60.000đ giảm
    Lợi nhuận sau = (1.200.000 × 45%) - 60.000 = 480.000đ
    Tỷ lệ = 480.000 / 1.200.000 = 40% ✓
  
  - COUPON10: 1.200.000 × 10% = 120.000đ giảm
    Lợi nhuận sau = (1.200.000 × 45%) - 120.000 = 420.000đ
    Tỷ lệ = 420.000 / 1.200.000 = 35% ✓
  
  - COUPON15: 1.200.000 × 15% = 180.000đ giảm
    Lợi nhuận sau = (1.200.000 × 45%) - 180.000 = 360.000đ
    Tỷ lệ = 360.000 / 1.200.000 = 30% ✓

Bước 5: Chọn tốt nhất
  → Hiển thị COUPON15 (15% - cao nhất)
```

### Ví Dụ 2: Đơn 1.800.000đ (Có Freeship)
```
Bước 1: Lọc cơ bản
  ✓ COUPON5, COUPON10, COUPON15, COUPON20 đủ điều kiện

Bước 4: Xử lý Freeship
  - Giới hạn tối đa 15%
  - Loại bỏ COUPON20, COUPON25

Bước 3: Tính lợi nhuận sau giảm (với COUPON15)
  - COUPON15: 1.800.000 × 15% = 270.000đ giảm
    Lợi nhuận sau = (1.800.000 × 45%) - 270.000 = 540.000đ
    Tỷ lệ = 540.000 / 1.800.000 = 30% ✓

Bước 5: Chọn tốt nhất
  → Hiển thị COUPON15 (15% - cao nhất trong giới hạn)
```

---

## 5. THÔNG BÁO GỢI Ý CHO KHÁCH

### Khi Đủ Điều Kiện
```
🏷 Giảm giá khả dụng cho bạn:
- COUPON15 – Giảm 15% (áp dụng đơn từ 1.000.000đ)
  Tiết kiệm: 180.000đ
```

### Khi Chưa Đủ Tiền
```
💰 Mua thêm 200.000đ để được giảm 10% (COUPON10)
💰 Mua thêm 800.000đ để được giảm 15% (COUPON15)
```

### Khi Có Freeship
```
⚠️ Với đơn hàng freeship, giảm giá tối đa 15%
🏷 Giảm giá khả dụng:
- COUPON15 – Giảm 15% (áp dụng đơn từ 1.000.000đ)
```

---

## 6. CẬP NHẬT TỰ ĐỘNG

### Khi Nào Cập Nhật Danh Sách Coupon?
1. **Khi thay đổi giỏ hàng** (thêm/xóa sản phẩm)
2. **Khi thay đổi số lượng sản phẩm**
3. **Khi áp dụng/xóa freeship**
4. **Khi chọn/bỏ chọn coupon**

### Cách Cập Nhật
- **Client-side**: AJAX call khi giỏ hàng thay đổi
- **Server-side**: API endpoint `/Coupon/GetAvailableCoupons`
- **Tham số**: `cartTotal`, `hasFreeship`, `productMargins`
- **Trả về**: Danh sách coupon khả dụng + thông báo gợi ý

---

## 7. CÔNG THỨC TÍNH TOÁN

```
Lợi nhuận tối thiểu cho phép = 20%
Biên lợi nhuận trung bình = 45%
Chi phí vận hành = 15%
Mục tiêu lợi nhuận = 25%

Công thức kiểm tra coupon:
  Lợi nhuận sau = (Tổng tiền × 45%) - (Tổng tiền × Phần trăm coupon)
  Tỷ lệ lợi nhuận = Lợi nhuận sau / Tổng tiền
  
  Điều kiện: Tỷ lệ lợi nhuận ≥ 20%
  
  Nếu có freeship:
    Phần trăm coupon ≤ 15%
```

---

## 8. TÓNG HỢP QUYẾT ĐỊNH

```
FOR EACH coupon IN database:
  IF coupon.ExpiryDate > NOW AND coupon.Quantity > 0 AND coupon.IsActive:
    IF cartTotal >= coupon.MinimumAmount:
      IF ALL products.Margin >= 30%:
        profitAfter = (cartTotal × 0.45) - (cartTotal × coupon.Discount%)
        profitRatio = profitAfter / cartTotal
        
        IF profitRatio >= 0.20:
          IF hasFreeship AND coupon.Discount% > 15%:
            SKIP this coupon
          ELSE:
            ADD to availableCoupons
            
SELECT coupon WITH MAX discount% FROM availableCoupons
DISPLAY to customer
```

