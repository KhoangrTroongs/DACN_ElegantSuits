# 📝 VÍ DỤ THỰC TẾ ÁP DỤNG COUPON

## VÍ DỤ 1: Đơn 500.000đ (Không Freeship, Margin OK)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 500.000đ
Freeship: Không
Sản phẩm: Áo vest (Margin 45%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- COUPON5: 500.000 >= 300.000 ✓
- COUPON10: 500.000 < 700.000 ✗
- COUPON15: 500.000 < 1.000.000 ✗
- COUPON20: 500.000 < 1.500.000 ✗
- COUPON25: 500.000 < 2.000.000 ✗

**Bước 2: Kiểm tra margin**
- Áo vest: 45% >= 30% ✓

**Bước 3: Tính lợi nhuận (COUPON5)**
- Tiền giảm: 500.000 × 5% = 25.000đ
- Lợi nhuận gốc: 500.000 × 45% = 225.000đ
- Lợi nhuận sau: 225.000 - 25.000 = 200.000đ
- Tỷ lệ: 200.000 / 500.000 = 40% >= 20% ✓

**Bước 4: Freeship** - Không có

**Bước 5: Chọn tốt nhất**
- Chỉ có COUPON5 → Hiển thị COUPON5

### Kết Quả Hiển Thị
```
🏷️ Giảm giá khả dụng cho bạn:
   COUPON5 – Giảm 5%
   Tiết kiệm: 25.000đ
   (Áp dụng cho đơn từ 300.000đ)
```

---

## VÍ DỤ 2: Đơn 1.200.000đ (Không Freeship, Margin OK)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 1.200.000đ
Freeship: Không
Sản phẩm: Áo vest (Margin 45%), Quần (Margin 50%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- COUPON5: 1.200.000 >= 300.000 ✓
- COUPON10: 1.200.000 >= 700.000 ✓
- COUPON15: 1.200.000 >= 1.000.000 ✓
- COUPON20: 1.200.000 < 1.500.000 ✗
- COUPON25: 1.200.000 < 2.000.000 ✗

**Bước 2: Kiểm tra margin**
- Áo vest: 45% >= 30% ✓
- Quần: 50% >= 30% ✓

**Bước 3: Tính lợi nhuận**

| Coupon | Giảm | Tiền Giảm | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|---------------|-------|--------|
| COUPON5 | 5% | 60.000đ | 480.000đ | 40% | ✓ |
| COUPON10 | 10% | 120.000đ | 420.000đ | 35% | ✓ |
| COUPON15 | 15% | 180.000đ | 360.000đ | 30% | ✓ |

**Bước 4: Freeship** - Không có

**Bước 5: Chọn tốt nhất**
- COUPON15 có % cao nhất → Hiển thị COUPON15

### Kết Quả Hiển Thị
```
🏷️ Giảm giá khả dụng cho bạn:
   COUPON15 – Giảm 15%
   Tiết kiệm: 180.000đ
   (Áp dụng cho đơn từ 1.000.000đ)
```

---

## VÍ DỤ 3: Đơn 1.800.000đ (Có Freeship, Margin OK)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 1.800.000đ
Freeship: Có
Sản phẩm: Áo vest (Margin 45%), Quần (Margin 50%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- COUPON5: 1.800.000 >= 300.000 ✓
- COUPON10: 1.800.000 >= 700.000 ✓
- COUPON15: 1.800.000 >= 1.000.000 ✓
- COUPON20: 1.800.000 >= 1.500.000 ✓
- COUPON25: 1.800.000 < 2.000.000 ✗

**Bước 2: Kiểm tra margin**
- Áo vest: 45% >= 30% ✓
- Quần: 50% >= 30% ✓

**Bước 3: Tính lợi nhuận**

| Coupon | Giảm | Tiền Giảm | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|---------------|-------|--------|
| COUPON5 | 5% | 90.000đ | 720.000đ | 40% | ✓ |
| COUPON10 | 10% | 180.000đ | 630.000đ | 35% | ✓ |
| COUPON15 | 15% | 270.000đ | 540.000đ | 30% | ✓ |
| COUPON20 | 20% | 360.000đ | 450.000đ | 25% | ✓ |

**Bước 4: Freeship** - Có
- COUPON20: 20% > 15% ✗ Loại bỏ
- COUPON25: Đã loại ở bước 1

**Bước 5: Chọn tốt nhất**
- COUPON15 có % cao nhất (trong giới hạn 15%) → Hiển thị COUPON15

### Kết Quả Hiển Thị
```
⚠️ Với đơn hàng freeship, giảm giá tối đa 15%

🏷️ Giảm giá khả dụng:
   COUPON15 – Giảm 15%
   Tiết kiệm: 270.000đ
   (Áp dụng cho đơn từ 1.000.000đ)
```

---

## VÍ DỤ 4: Đơn 1.200.000đ (Margin Không Đủ)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 1.200.000đ
Freeship: Không
Sản phẩm: Áo vest (Margin 45%), Áo sale (Margin 20%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- COUPON5, COUPON10, COUPON15 đủ tiền ✓

**Bước 2: Kiểm tra margin**
- Áo vest: 45% >= 30% ✓
- Áo sale: 20% < 30% ✗ **LOẠI BỎ TẤT CẢ COUPON**

### Kết Quả Hiển Thị
```
❌ Sản phẩm này không áp dụng mã giảm giá
   (Biên lợi nhuận không đủ)
```

---

## VÍ DỤ 5: Đơn 250.000đ (Chưa Đủ Tiền)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 250.000đ
Freeship: Không
Sản phẩm: Áo vest (Margin 45%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- COUPON5: 250.000 < 300.000 ✗
- Tất cả coupon khác cũng không đủ ✗

### Kết Quả Hiển Thị
```
💰 Mua thêm 50.000đ để được giảm 5% (COUPON5)
💰 Mua thêm 450.000đ để được giảm 10% (COUPON10)
💰 Mua thêm 750.000đ để được giảm 15% (COUPON15)
```

---

## VÍ DỤ 6: Đơn 2.500.000đ (Tối Đa)

### Dữ Liệu Đầu Vào
```
Tổng tiền: 2.500.000đ
Freeship: Không
Sản phẩm: Áo vest (Margin 45%), Quần (Margin 50%)
```

### Quy Trình Kiểm Tra

**Bước 1: Lọc cơ bản**
- Tất cả coupon đủ tiền ✓

**Bước 2: Kiểm tra margin**
- Tất cả sản phẩm >= 30% ✓

**Bước 3: Tính lợi nhuận**

| Coupon | Giảm | Tiền Giảm | Lợi Nhuận Sau | Tỷ Lệ | Hợp Lệ |
|--------|------|----------|---------------|-------|--------|
| COUPON5 | 5% | 125.000đ | 1.000.000đ | 40% | ✓ |
| COUPON10 | 10% | 250.000đ | 875.000đ | 35% | ✓ |
| COUPON15 | 15% | 375.000đ | 750.000đ | 30% | ✓ |
| COUPON20 | 20% | 500.000đ | 625.000đ | 25% | ✓ |
| COUPON25 | 25% | 625.000đ | 500.000đ | 20% | ✓ |

**Bước 4: Freeship** - Không có

**Bước 5: Chọn tốt nhất**
- COUPON25 có % cao nhất → Hiển thị COUPON25

### Kết Quả Hiển Thị
```
🏷️ Giảm giá khả dụng cho bạn:
   COUPON25 – Giảm 25%
   Tiết kiệm: 625.000đ
   (Áp dụng cho đơn từ 2.000.000đ)
```

---

## BẢNG TÓNG HỢP KẾT QUẢ

| Tổng Tiền | Freeship | Margin | Coupon Hiển Thị | Tiết Kiệm | Ghi Chú |
|-----------|----------|--------|-----------------|-----------|---------|
| 250.000đ | Không | OK | Không | - | Chưa đủ tiền |
| 500.000đ | Không | OK | COUPON5 | 25.000đ | Tiết kiệm 5% |
| 1.200.000đ | Không | OK | COUPON15 | 180.000đ | Tiết kiệm 15% |
| 1.200.000đ | Không | Không OK | Không | - | Margin < 30% |
| 1.800.000đ | Có | OK | COUPON15 | 270.000đ | Giới hạn 15% |
| 2.500.000đ | Không | OK | COUPON25 | 625.000đ | Tiết kiệm 25% |

