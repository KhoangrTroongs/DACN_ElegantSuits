# Coupon System - Quick Start Guide

## 🚀 Getting Started

### For Administrators

#### 1. Access Admin Panel
- Log in with Administrator account
- Navigate to: **Admin Panel → Quản Lý Mã Giảm Giá**
- Or directly visit: `/Coupon/Index`

#### 2. Create a New Coupon
1. Click **"Tạo Mã Giảm Giá"** button
2. Fill in the form:
   - **Mã giảm giá**: Enter coupon code (e.g., `SUMMER2024`, `WELCOME10`)
   - **Phần trăm giảm**: Enter discount percentage (0-100)
   - **Số lượng**: 
     - Enter a number for limited usage (e.g., 100)
     - Enter `-1` for unlimited usage
     - Enter `0` to mark as expired
   - **Ngày hết hạn**: Select expiry date and time
   - **Kích hoạt**: Check to activate the coupon
3. Click **"Tạo Mã Giảm Giá"**

#### 3. View All Coupons
- Go to **Coupon/Index**
- See all coupons with:
  - Code
  - Discount percentage
  - Remaining quantity
  - Expiry date
  - Status (Active/Expired/Depleted)

#### 4. Edit a Coupon
1. Click **"Sửa"** button on any coupon
2. Update:
   - Discount percentage
   - Quantity
   - Expiry date
   - Active status
3. Note: **Coupon code cannot be changed**
4. Click **"Cập Nhật"**

#### 5. Delete a Coupon
1. Click **"Xóa"** button on any coupon
2. Confirm deletion on the confirmation page
3. Click **"Xóa Mã Giảm Giá"**

---

### For Customers

#### 1. Apply Coupon During Checkout
1. Add items to cart
2. Go to checkout page
3. In the order summary section, find **"Mã Giảm Giá"** field
4. Enter the coupon code (e.g., `SUMMER2024`)
5. Click **"Áp Dụng"** button

#### 2. See Discount Applied
- If coupon is valid:
  - ✅ Success message appears
  - Discount amount is displayed
  - Total price is updated
- If coupon is invalid:
  - ❌ Error message appears
  - Discount is not applied

#### 3. Complete Order
1. Fill in shipping address
2. Add order notes (optional)
3. Click **"Đặt Hàng"** to complete purchase
4. Coupon quantity is automatically decremented

---

## 📋 Coupon Status Indicators

| Status | Meaning | Can Use? |
|--------|---------|----------|
| 🟢 Còn hiệu lực | Active and available | ✅ Yes |
| 🔴 Đã hết hạn | Past expiry date | ❌ No |
| 🔴 Đã hết số lượng | Quantity = 0 | ❌ No |
| ⚫ Không kích hoạt | IsActive = false | ❌ No |

---

## 💡 Example Scenarios

### Scenario 1: Limited Time Promotion
```
Code: SUMMER2024
Discount: 20%
Quantity: 100
Expiry: 2024-12-31 23:59
Active: Yes

Result: First 100 customers get 20% off until Dec 31
```

### Scenario 2: Unlimited Loyalty Coupon
```
Code: LOYAL10
Discount: 10%
Quantity: -1 (unlimited)
Expiry: 2025-12-31 23:59
Active: Yes

Result: Loyal customers can use this coupon unlimited times
```

### Scenario 3: Flash Sale
```
Code: FLASH50
Discount: 50%
Quantity: 50
Expiry: 2024-10-28 18:00
Active: Yes

Result: First 50 customers get 50% off (limited time)
```

---

## ⚠️ Error Messages & Solutions

### "Mã giảm giá không tồn tại"
**Problem**: Coupon code doesn't exist in database
**Solution**: 
- Check spelling of coupon code
- Verify coupon was created by admin
- Ask admin to create the coupon

### "Mã giảm giá đã hết hạn"
**Problem**: Coupon expiry date has passed
**Solution**:
- Use a different coupon
- Ask admin to extend expiry date
- Ask admin to create a new coupon

### "Mã giảm giá đã hết số lượng"
**Problem**: Coupon quantity is 0 (depleted)
**Solution**:
- Use a different coupon
- Ask admin to increase quantity
- Ask admin to create a new coupon

### "Mã giảm giá không khả dụng"
**Problem**: Coupon is inactive (IsActive = false)
**Solution**:
- Use a different coupon
- Ask admin to activate the coupon

---

## 🔧 Technical Details

### Coupon Code Rules
- ✅ Case-insensitive (SUMMER2024 = summer2024)
- ✅ Stored in uppercase in database
- ✅ Must be unique
- ✅ Max 50 characters

### Discount Calculation
```
Discount Amount = (Order Total × Discount Percentage) / 100
Final Total = Order Total - Discount Amount

Example:
Order Total: 1,000,000đ
Discount: 20%
Discount Amount: 1,000,000 × 20 / 100 = 200,000đ
Final Total: 1,000,000 - 200,000 = 800,000đ
```

### Quantity Management
- **-1**: Unlimited usage (never decrements)
- **0**: Expired/depleted (cannot be used)
- **> 0**: Limited usage (decrements by 1 after each order)

---

## 📊 Admin Dashboard Features

### Coupon List View
- Sort by: Code, Discount %, Quantity, Expiry Date
- Filter by: Status (Active/Expired/Depleted)
- Quick actions: Edit, Delete
- Visual indicators for status

### Coupon Details
- Creation date
- Last update date
- Current quantity remaining
- Days until expiry
- Active/Inactive status

---

## 🎯 Best Practices

### For Administrators
1. ✅ Set realistic expiry dates
2. ✅ Use meaningful coupon codes
3. ✅ Monitor coupon usage
4. ✅ Deactivate expired coupons
5. ✅ Create seasonal promotions
6. ✅ Test coupons before launching

### For Customers
1. ✅ Check coupon validity before checkout
2. ✅ Note expiry dates
3. ✅ Share valid coupons with friends
4. ✅ Apply coupon before confirming order
5. ✅ Check discount amount before paying

---

## 🆘 Troubleshooting

### Coupon not applying?
1. Check coupon code spelling
2. Verify coupon is active
3. Check expiry date
4. Check quantity (not 0)
5. Refresh page and try again

### Discount not showing?
1. Click "Áp Dụng" button
2. Wait for AJAX response
3. Check for error message
4. Try different coupon

### Can't create coupon?
1. Verify you're logged in as Administrator
2. Check all required fields are filled
3. Ensure coupon code is unique
4. Check expiry date is in future

---

## 📞 Support

For issues or questions:
- Contact Administrator
- Check error messages carefully
- Review this guide
- Check system logs

---

**Last Updated**: October 28, 2024
**Version**: 1.0
**Status**: Production Ready ✅

