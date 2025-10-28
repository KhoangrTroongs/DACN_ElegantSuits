# Coupon System - Architecture & Design

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Admin Panel                          Customer Checkout      │
│  ├─ Views/Coupon/Index.cshtml        ├─ Checkout.cshtml    │
│  ├─ Views/Coupon/Create.cshtml       └─ AJAX Validation    │
│  ├─ Views/Coupon/Edit.cshtml                                │
│  └─ Views/Coupon/Delete.cshtml                              │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   CONTROLLER LAYER                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  CouponController                  ShoppingCartController   │
│  ├─ Index()                        ├─ Checkout()           │
│  ├─ Create()                       ├─ ValidateCoupon()     │
│  ├─ Edit()                         └─ DecrementQuantity()  │
│  └─ Delete()                                                │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ICouponService / CouponService                             │
│  ├─ GetAllCouponsAsync()                                    │
│  ├─ GetCouponByIdAsync()                                    │
│  ├─ GetCouponByCodeAsync()                                  │
│  ├─ AddCouponAsync()                                        │
│  ├─ UpdateCouponAsync()                                     │
│  ├─ DeleteCouponAsync()                                     │
│  ├─ ValidateCouponAsync() ← Core validation logic           │
│  └─ DecrementCouponQuantityAsync()                          │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  REPOSITORY LAYER                           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ICouponRepository / EFCouponRepository                      │
│  ├─ GetAllCouponsAsync()                                    │
│  ├─ GetCouponByIdAsync()                                    │
│  ├─ GetCouponByCodeAsync()                                  │
│  ├─ AddCouponAsync()                                        │
│  ├─ UpdateCouponAsync()                                     │
│  ├─ DeleteCouponAsync()                                     │
│  ├─ CouponExistsAsync()                                     │
│  └─ CouponCodeExistsAsync()                                 │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   DATA ACCESS LAYER                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Entity Framework Core                                       │
│  ├─ DbContext: ApplicationDbContext                         │
│  ├─ DbSet<Coupon>                                           │
│  └─ DbSet<Order> (with CouponCode, DiscountAmount)         │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   DATABASE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  SQL Server                                                  │
│  ├─ Coupons Table                                           │
│  └─ Orders Table (updated)                                  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Flow Diagram

### Admin Creating a Coupon
```
Admin UI (Create Form)
    ↓
CouponController.Create(POST)
    ↓
CouponService.AddCouponAsync()
    ├─ Validate coupon code doesn't exist
    ├─ Convert code to uppercase
    └─ Set CreatedAt timestamp
    ↓
ICouponRepository.AddCouponAsync()
    ├─ Convert code to uppercase
    └─ Save to database
    ↓
Database (Coupons Table)
    ↓
Redirect to Index with success message
```

### Customer Applying a Coupon
```
Checkout Page
    ↓
User enters coupon code
    ↓
Click "Áp Dụng" button
    ↓
AJAX POST to ShoppingCartController.ValidateCoupon()
    ↓
CouponService.ValidateCouponAsync()
    ├─ Check coupon exists
    ├─ Check coupon is active
    ├─ Check coupon not expired
    ├─ Check coupon quantity available
    └─ Return CouponValidationResult
    ↓
JavaScript updates UI
    ├─ Show discount amount
    ├─ Update total price
    └─ Display success/error message
    ↓
User submits order
    ↓
ShoppingCartController.Checkout(POST)
    ├─ Validate coupon again
    ├─ Calculate discount
    ├─ Create order with CouponCode & DiscountAmount
    └─ Decrement coupon quantity
    ↓
Database updates
    ├─ Order created
    └─ Coupon quantity decremented
```

---

## Validation Flow

```
ValidateCouponAsync(code)
    ↓
Step 1: Get coupon by code (case-insensitive)
    ├─ If not found → Return "Mã giảm giá không tồn tại"
    └─ Continue
    ↓
Step 2: Check IsActive
    ├─ If false → Return "Mã giảm giá không khả dụng"
    └─ Continue
    ↓
Step 3: Check ExpiryDate
    ├─ If DateTime.Now > ExpiryDate → Return "Mã giảm giá đã hết hạn"
    └─ Continue
    ↓
Step 4: Check Quantity
    ├─ If Quantity == 0 → Return "Mã giảm giá đã hết số lượng"
    └─ Continue
    ↓
Step 5: All checks passed
    └─ Return IsValid = true with Coupon data
```

---

## Database Schema

### Coupons Table
```
Column Name          | Type           | Constraints
─────────────────────┼────────────────┼──────────────────────
Id                   | INT            | PRIMARY KEY, IDENTITY
Code                 | NVARCHAR(50)   | NOT NULL, UNIQUE
Quantity             | INT            | NOT NULL
DiscountPercentage   | DECIMAL(5,2)   | NOT NULL
ExpiryDate           | DATETIME       | NOT NULL
IsActive             | BIT            | NOT NULL
CreatedAt            | DATETIME       | NOT NULL
UpdatedAt            | DATETIME       | NULL
```

### Orders Table (Updated)
```
New Columns:
─────────────────────┼────────────────┼──────────────────────
CouponCode           | NVARCHAR(50)   | NULL
DiscountAmount       | DECIMAL(18,2)  | NOT NULL, DEFAULT 0
```

---

## Key Design Patterns Used

### 1. **Repository Pattern**
- Abstraction of data access logic
- Easy to test and maintain
- Separation of concerns

### 2. **Service Layer Pattern**
- Business logic encapsulation
- Validation and error handling
- DTO mapping

### 3. **Dependency Injection**
- Loose coupling
- Easy to mock for testing
- Registered in Program.cs

### 4. **DTO Pattern**
- Data transfer between layers
- Validation at DTO level
- Computed properties for display

### 5. **AJAX Pattern**
- Real-time validation without page reload
- Better user experience
- Asynchronous operations

---

## Security Considerations

✅ **Authorization**: [Authorize(Roles = "Administrator")] on admin pages
✅ **Case-Insensitive Codes**: Prevents duplicate codes with different cases
✅ **Validation**: Multiple validation checks before applying discount
✅ **Quantity Management**: Prevents overuse of limited coupons
✅ **Expiry Dates**: Automatic expiration handling
✅ **Active Status**: Ability to deactivate coupons without deletion

---

## Performance Considerations

✅ **Async/Await**: All database operations are asynchronous
✅ **Efficient Queries**: Direct lookups by code or ID
✅ **Caching**: Could be added for frequently accessed coupons
✅ **Indexing**: Unique index on Code column for fast lookups

---

## Future Enhancements

- [ ] Coupon usage statistics and analytics
- [ ] Bulk coupon generation
- [ ] Coupon categories/types
- [ ] Minimum order amount requirements
- [ ] Product-specific coupons
- [ ] User-specific coupons
- [ ] Coupon usage history tracking
- [ ] Email notifications for expiring coupons

