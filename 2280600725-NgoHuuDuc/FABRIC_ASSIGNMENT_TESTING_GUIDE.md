# Fabric Assignment Testing Guide

## Quick Start

### Access the Application
1. Application is running at: `https://localhost:5001` (or `http://localhost:5000`)
2. Log in with administrator account
3. Navigate to "Quản lý sản phẩm" (Product Management)

---

## Test Scenario 1: Create Product with Fabrics

### Steps
1. Click "Thêm sản phẩm mới" (Add New Product)
2. Fill in product details:
   - **Tên sản phẩm:** Test Product 1
   - **Giá:** 500000
   - **Danh mục:** Select any category
   - **Số lượng:** 10
3. Scroll down to "Vải (không bắt buộc)" section
4. Select 3-5 fabrics by checking their checkboxes
5. Click "Lưu" (Save)

### Expected Results
✅ Product is created successfully
✅ Selected fabrics are associated with the product
✅ No error messages appear
✅ Redirected to product list

---

## Test Scenario 2: Edit Product and Change Fabrics

### Steps
1. Go to Product Management
2. Click "Sửa" (Edit) on any product
3. Scroll to "Vải (không bắt buộc)" section
4. Notice which fabrics are pre-checked (currently assigned)
5. Uncheck 1-2 fabrics
6. Check 2-3 new fabrics
7. Click "Lưu" (Save)

### Expected Results
✅ Product is updated successfully
✅ Old fabric associations are removed
✅ New fabric associations are created
✅ Pre-checked fabrics match previously assigned fabrics
✅ Changes are reflected in database

---

## Test Scenario 3: Verify Fabrics Remain Available

### Steps
1. Create Product A with Fabrics: Silk, Cotton, Wool
2. Create Product B with Fabrics: Silk, Linen
3. Edit Product A and check if Silk is still available
4. Edit Product B and check if Silk is still available
5. Verify both products can use the same fabrics

### Expected Results
✅ Fabrics are NOT removed from available list after assignment
✅ Multiple products can use the same fabrics
✅ No "fabric disappears" issue
✅ Many-to-many relationship works correctly

---

## Test Scenario 4: Create Product Without Fabrics

### Steps
1. Create a new product
2. Leave "Vải (không bắt buộc)" section empty (no checkboxes checked)
3. Save the product

### Expected Results
✅ Product is created successfully
✅ No fabrics are associated
✅ No error messages appear
✅ Product can be edited later to add fabrics

---

## Test Scenario 5: Edit Product to Add Fabrics

### Steps
1. Find a product with no fabrics assigned
2. Click "Sửa" (Edit)
3. Scroll to fabric section
4. Select 2-3 fabrics
5. Save

### Expected Results
✅ Fabrics are successfully added to the product
✅ Product now shows selected fabrics
✅ No previous data is lost

---

## Test Scenario 6: Edit Product to Remove All Fabrics

### Steps
1. Find a product with fabrics assigned
2. Click "Sửa" (Edit)
3. Uncheck all selected fabrics
4. Save

### Expected Results
✅ All fabric associations are removed
✅ Product is updated successfully
✅ Product can be edited again to add fabrics

---

## Verification Checklist

### Database Verification
```sql
-- Check product-fabric associations
SELECT p.Id, p.Name, f.Id, f.Name 
FROM Products p
LEFT JOIN FabricProducts fp ON p.Id = fp.ProductId
LEFT JOIN Fabrics f ON fp.FabricId = f.Id
WHERE p.Id = [ProductId]
```

### UI Verification
- [ ] Fabric section displays in Create form
- [ ] Fabric section displays in Edit form
- [ ] Checkboxes are properly checked/unchecked
- [ ] Fabric names display correctly
- [ ] Fabric group names display in parentheses
- [ ] Scrollable container works (max-height: 300px)
- [ ] Vietnamese text displays correctly
- [ ] No JavaScript errors in browser console

### Functionality Verification
- [ ] Can create product with fabrics
- [ ] Can edit product and change fabrics
- [ ] Can remove all fabrics from product
- [ ] Can add fabrics to product without fabrics
- [ ] Fabrics remain available after assignment
- [ ] Multiple products can share same fabrics
- [ ] Database associations are correct

---

## Troubleshooting

### Issue: Fabrics not showing in form
**Solution:** 
- Ensure fabrics exist in database
- Check if FabricService is properly injected
- Verify database connection

### Issue: Checkboxes not pre-selected on Edit
**Solution:**
- Check if SelectedFabricIds is populated correctly
- Verify fabric IDs match between database and form
- Check browser console for JavaScript errors

### Issue: Fabrics disappear after assignment
**Solution:**
- This should NOT happen with new design
- If it does, check ProductFabricAdminController is deleted
- Verify Edit view is using correct logic

---

## Performance Notes
- Fabric list is loaded on every Create/Edit page load
- For large fabric catalogs (100+), consider pagination
- Current implementation suitable for up to 500 fabrics

---

## Next Steps
1. Run all test scenarios
2. Verify database integrity
3. Test on different browsers
4. Test on mobile devices
5. Deploy to production

---

**Last Updated:** October 23, 2025
**Status:** Ready for Testing

