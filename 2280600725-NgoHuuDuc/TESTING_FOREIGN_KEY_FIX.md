# Testing Guide - Foreign Key Constraint Fix

## Overview
This guide provides step-by-step instructions to verify that the foreign key constraint error has been fixed.

---

## Prerequisites
- Application is running (`dotnet watch run`)
- Logged in as Administrator
- Database has at least 3 fabrics available
- Database has at least 1 category available

---

## Test Scenario 1: Create Product with Single Fabric

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Click **Thêm sản phẩm mới** (Add New Product)
3. Fill in product details:
   - **Tên sản phẩm:** Test Product 1
   - **Giá:** 500000
   - **Danh mục:** Select any category
   - **Số lượng:** 10
4. Scroll to **Vải (không bắt buộc)** section
5. Select **1 fabric** by checking its checkbox
6. Click **Lưu** (Save)

### Expected Results
✅ Product created successfully
✅ Redirected to product list
✅ Success message: "Sản phẩm đã được thêm thành công."
✅ No error messages
✅ No foreign key constraint error

### Verification
```sql
-- Verify fabric association was created
SELECT p.Id, p.Name, f.Id, f.Name 
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Name = 'Test Product 1'
```

---

## Test Scenario 2: Create Product with Multiple Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Click **Thêm sản phẩm mới** (Add New Product)
3. Fill in product details:
   - **Tên sản phẩm:** Test Product 2
   - **Giá:** 750000
   - **Danh mục:** Select any category
   - **Số lượng:** 5
4. Scroll to **Vải (không bắt buộc)** section
5. Select **3-5 fabrics** by checking their checkboxes
6. Click **Lưu** (Save)

### Expected Results
✅ Product created successfully
✅ All selected fabrics are associated
✅ Success message displayed
✅ No foreign key constraint error

### Verification
```sql
-- Verify all fabric associations were created
SELECT p.Id, p.Name, COUNT(f.Id) as FabricCount
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Name = 'Test Product 2'
GROUP BY p.Id, p.Name
```

---

## Test Scenario 3: Edit Product and Add Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Find **Test Product 1** (created in Scenario 1)
3. Click **Sửa** (Edit)
4. Scroll to **Vải (không bắt buộc)** section
5. Notice which fabric is pre-checked (currently assigned)
6. Check **2-3 additional fabrics**
7. Click **Lưu** (Save)

### Expected Results
✅ Product updated successfully
✅ New fabrics are associated
✅ Old fabric associations are maintained
✅ Success message: "Sản phẩm đã được cập nhật thành công."
✅ No foreign key constraint error

### Verification
```sql
-- Verify fabric associations were updated
SELECT p.Id, p.Name, f.Id, f.Name 
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Name = 'Test Product 1'
ORDER BY f.Id
```

---

## Test Scenario 4: Edit Product and Remove Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Find **Test Product 2** (created in Scenario 2)
3. Click **Sửa** (Edit)
4. Scroll to **Vải (không bắt buộc)** section
5. Uncheck **1-2 fabrics** that are currently selected
6. Click **Lưu** (Save)

### Expected Results
✅ Product updated successfully
✅ Unchecked fabrics are removed
✅ Checked fabrics remain associated
✅ Success message displayed
✅ No foreign key constraint error

### Verification
```sql
-- Verify fabric associations were removed
SELECT p.Id, p.Name, COUNT(f.Id) as FabricCount
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Name = 'Test Product 2'
GROUP BY p.Id, p.Name
```

---

## Test Scenario 5: Create Product Without Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Click **Thêm sản phẩm mới** (Add New Product)
3. Fill in product details
4. Scroll to **Vải (không bắt buộc)** section
5. **Do NOT select any fabrics**
6. Click **Lưu** (Save)

### Expected Results
✅ Product created successfully
✅ No fabrics are associated
✅ Success message displayed
✅ No error messages

### Verification
```sql
-- Verify no fabric associations were created
SELECT p.Id, p.Name, COUNT(f.Id) as FabricCount
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Name LIKE 'Test Product%'
GROUP BY p.Id, p.Name
```

---

## Test Scenario 6: Edit Product to Add Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Find the product created in Scenario 5 (no fabrics)
3. Click **Sửa** (Edit)
4. Scroll to **Vải (không bắt buộc)** section
5. Select **2-3 fabrics**
6. Click **Lưu** (Save)

### Expected Results
✅ Product updated successfully
✅ Fabrics are now associated
✅ Success message displayed
✅ No foreign key constraint error

---

## Test Scenario 7: Verify Pre-selected Fabrics

### Steps
1. Navigate to **Quản lý sản phẩm** (Product Management)
2. Find **Test Product 2** (has multiple fabrics)
3. Click **Sửa** (Edit)
4. Scroll to **Vải (không bắt buộc)** section
5. Verify that **all previously selected fabrics are checked**
6. Verify that **unselected fabrics are unchecked**

### Expected Results
✅ All previously assigned fabrics are pre-checked
✅ Unassigned fabrics are unchecked
✅ Checkboxes accurately reflect database state

---

## Database Integrity Check

### Run This Query
```sql
-- Check for orphaned fabric associations
SELECT fp.Id, fp.ProductId, fp.FabricId, p.Name, f.Name
FROM FabricProducts fp
LEFT JOIN Products p ON fp.ProductId = p.Id
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Id IS NULL OR f.Id IS NULL
```

### Expected Result
✅ **No rows returned** - All associations are valid

---

## Browser Console Check

### Steps
1. Open browser Developer Tools (F12)
2. Go to **Console** tab
3. Perform all test scenarios above
4. Check for any JavaScript errors

### Expected Results
✅ No red error messages
✅ No warnings related to fabric assignment
✅ Console is clean

---

## Error Handling Test

### Steps
1. Open SQL Server Management Studio
2. Delete a fabric from the Fabrics table (note its ID)
3. In the application, try to create a product with that fabric ID
4. **Note:** This requires manual form manipulation or database modification

### Expected Results
✅ User-friendly error message displayed
✅ Error message indicates fabric doesn't exist
✅ Form is reloaded with all data intact
✅ No foreign key constraint error in logs

---

## Performance Check

### Steps
1. Create a product with 10+ fabrics
2. Edit the product and change fabric selection
3. Monitor response time

### Expected Results
✅ Response time < 2 seconds
✅ No timeout errors
✅ Smooth user experience

---

## Final Verification Checklist

- [ ] Test Scenario 1 passed
- [ ] Test Scenario 2 passed
- [ ] Test Scenario 3 passed
- [ ] Test Scenario 4 passed
- [ ] Test Scenario 5 passed
- [ ] Test Scenario 6 passed
- [ ] Test Scenario 7 passed
- [ ] Database integrity check passed
- [ ] Browser console is clean
- [ ] Error handling works correctly
- [ ] Performance is acceptable

---

## Troubleshooting

### Issue: Foreign Key Error Still Occurs
**Solution:**
1. Verify application was rebuilt: `dotnet build`
2. Verify application was restarted: `dotnet watch run`
3. Clear browser cache (Ctrl+Shift+Delete)
4. Check application logs for detailed error

### Issue: Fabrics Not Pre-selected on Edit
**Solution:**
1. Verify database has correct associations
2. Check browser console for JavaScript errors
3. Verify ProductViewModel is populated correctly

### Issue: Error Message Not Displayed
**Solution:**
1. Check if ModelState errors are being displayed in view
2. Verify error handling code is in place
3. Check application logs

---

## Success Criteria

✅ All test scenarios pass
✅ No foreign key constraint errors
✅ Database integrity maintained
✅ User-friendly error messages displayed
✅ Performance acceptable
✅ Browser console clean

---

**Status:** Ready for Testing
**Date:** October 23, 2025

