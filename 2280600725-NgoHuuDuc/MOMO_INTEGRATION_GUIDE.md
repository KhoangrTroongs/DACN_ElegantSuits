# Hướng Dẫn Tích Hợp MoMo Payment Gateway

## 1. Cập Nhật Database

Chạy script SQL sau để thêm các trường cần thiết cho thanh toán MoMo:

```sql
-- Mở SQL Server Management Studio và kết nối đến database WEBQLSP
-- Sau đó chạy file: SQL/AddMoMoPaymentFields.sql
```

Hoặc chạy trực tiếp trong Package Manager Console:
```powershell
# Trong Visual Studio, mở Tools > NuGet Package Manager > Package Manager Console
# Chạy lệnh sau:
sqlcmd -S VERON -d WEBQLSP -E -i "SQL/AddMoMoPaymentFields.sql"
```

## 2. Cấu Hình MoMo

Trong file `appsettings.json`, các thông tin MoMo đã được cấu hình:

```json
"MoMo": {
  "PartnerCode": "MOMOBKUN20180529",
  "AccessKey": "klm05TvNBzhg7h7j",
  "SecretKey": "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa",
  "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
  "ReturnUrl": "https://localhost:5001/Payment/MoMoReturn",
  "NotifyUrl": "https://localhost:5001/Payment/MoMoNotify"
}
```

**Lưu ý:** Đây là thông tin test của MoMo. Để sử dụng production, bạn cần:
1. Đăng ký tài khoản MoMo Business tại: https://business.momo.vn/
2. Lấy thông tin PartnerCode, AccessKey, SecretKey thực tế
3. Cập nhật Endpoint thành: `https://payment.momo.vn/v2/gateway/api/create`
4. Cập nhật ReturnUrl và NotifyUrl với domain thực tế của bạn

## 3. Các Tính Năng Đã Tích Hợp

### 3.1. Models
- `MoMoPaymentRequest`: Request gửi đến MoMo
- `MoMoPaymentResponse`: Response từ MoMo
- `MoMoPaymentResultRequest`: Kết quả thanh toán từ MoMo
- `PaymentStatus` enum: Trạng thái thanh toán (Pending, Paid, Failed, Refunded)

### 3.2. Services
- `IMoMoService` & `MoMoService`: Xử lý logic thanh toán MoMo
  - `CreatePaymentAsync()`: Tạo link thanh toán
  - `ValidateSignature()`: Xác thực chữ ký từ MoMo

### 3.3. Controllers
- `PaymentController`: Xử lý thanh toán
  - `CreateMoMoPayment()`: Tạo thanh toán MoMo
  - `MoMoReturn()`: Xử lý khi user quay lại từ MoMo
  - `MoMoNotify()`: Nhận IPN (Instant Payment Notification) từ MoMo

- `OrderController`: Xử lý đơn hàng
  - `OrderSuccess()`: Hiển thị đơn hàng thành công
  - `OrderFailed()`: Hiển thị đơn hàng thất bại

### 3.4. Views
- `Views/ShoppingCart/Checkout.cshtml`: Thêm tùy chọn thanh toán MoMo
- `Views/Order/OrderSuccess.cshtml`: Trang thành công
- `Views/Order/OrderFailed.cshtml`: Trang thất bại

## 4. Luồng Thanh Toán

1. **User chọn sản phẩm và vào trang Checkout**
   - Chọn phương thức thanh toán: COD hoặc MoMo
   - Nhập địa chỉ giao hàng và thông tin khác

2. **Nếu chọn MoMo:**
   - Hệ thống tạo đơn hàng với trạng thái Pending
   - Gọi `ShoppingCartController.ProcessMoMoPayment()`
   - Tạo link thanh toán MoMo qua `MoMoService.CreatePaymentAsync()`
   - Redirect user đến trang thanh toán MoMo

3. **User thanh toán trên MoMo:**
   - Quét QR code hoặc đăng nhập MoMo
   - Xác nhận thanh toán

4. **MoMo xử lý và trả kết quả:**
   - **IPN (Instant Payment Notification):** MoMo gọi `PaymentController.MoMoNotify()` để cập nhật trạng thái
   - **Return URL:** User được redirect về `PaymentController.MoMoReturn()`

5. **Hệ thống cập nhật đơn hàng:**
   - Nếu thành công: PaymentStatus = Paid, OrderStatus = Confirmed
   - Nếu thất bại: PaymentStatus = Failed
   - Redirect đến trang OrderSuccess hoặc OrderFailed

## 5. Test Thanh Toán MoMo

### 5.1. Sử dụng MoMo Test Environment
Với thông tin test đã cấu hình, bạn có thể test thanh toán mà không cần tài khoản MoMo thật.

### 5.2. Test Cases
1. **Thanh toán thành công:**
   - Tạo đơn hàng và chọn MoMo
   - Scan QR code hoặc nhập số điện thoại test
   - Xác nhận thanh toán
   - Kiểm tra đơn hàng có trạng thái Paid

2. **Thanh toán thất bại:**
   - Tạo đơn hàng và chọn MoMo
   - Hủy thanh toán trên trang MoMo
   - Kiểm tra đơn hàng có trạng thái Failed

3. **Timeout:**
   - Tạo đơn hàng và chọn MoMo
   - Không thực hiện thanh toán trong thời gian quy định
   - Kiểm tra xử lý timeout

## 6. Bảo Mật

### 6.1. Signature Validation
- Mọi request từ MoMo đều được validate signature bằng HMAC-SHA256
- Không tin tưởng dữ liệu nếu signature không hợp lệ

### 6.2. HTTPS
- Tất cả communication với MoMo phải qua HTTPS
- ReturnUrl và NotifyUrl phải là HTTPS

### 6.3. Secret Key
- Không commit SecretKey vào Git
- Sử dụng User Secrets hoặc Azure Key Vault cho production

## 7. Troubleshooting

### 7.1. Lỗi "Invalid signature"
- Kiểm tra SecretKey có đúng không
- Kiểm tra thứ tự các tham số khi tạo signature
- Kiểm tra encoding (UTF-8)

### 7.2. Lỗi "Order not found"
- Kiểm tra OrderId có tồn tại trong database không
- Kiểm tra format OrderId

### 7.3. IPN không được gọi
- Kiểm tra NotifyUrl có accessible từ internet không
- Sử dụng ngrok để expose localhost cho test
- Kiểm tra firewall

## 8. Production Checklist

- [ ] Đăng ký tài khoản MoMo Business
- [ ] Lấy thông tin PartnerCode, AccessKey, SecretKey production
- [ ] Cập nhật Endpoint production
- [ ] Cập nhật ReturnUrl và NotifyUrl với domain thực tế
- [ ] Enable HTTPS cho toàn bộ website
- [ ] Lưu SecretKey vào User Secrets hoặc Azure Key Vault
- [ ] Test kỹ trên môi trường staging
- [ ] Setup monitoring và logging
- [ ] Chuẩn bị xử lý refund (nếu cần)

## 9. Liên Hệ Hỗ Trợ

- MoMo Developer Portal: https://developers.momo.vn/
- MoMo Business Support: https://business.momo.vn/support
- Email: business@momo.vn

