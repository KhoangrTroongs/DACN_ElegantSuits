# Admin Fabric Management - Quick Reference Guide

## 🔐 Access Requirements
- Must be logged in as **Administrator**
- Role-based access control enforced
- All operations require anti-forgery tokens

## 📍 Navigation

### From Main Dashboard
1. Click **"Quản lý"** (Admin) in top navigation bar
2. Look for **palette icon** (🎨) in admin sidebar
3. Or expand **"Quản lý vải"** menu

### Admin Menu Structure
```
Quản lý vải (Fabric Management)
├── Nhóm vải (Fabric Groups)
├── Danh sách vải (Fabric List)
└── Gán vải cho sản phẩm (Assign Fabrics to Products)
```

## 📋 Fabric Groups Management

### View All Groups
- **Path:** Admin → Quản lý vải → Nhóm vải
- **Display:** Table with name, description, display order, fabric count
- **Actions:** Edit, Delete

### Create New Group
1. Click **"Tạo nhóm vải mới"** button
2. Fill in:
   - **Tên nhóm vải** (Group Name) - Required
   - **Mô tả** (Description) - Optional
   - **Thứ tự hiển thị** (Display Order) - Default: 1
3. Click **"Tạo nhóm vải"** to save

### Edit Group
1. Click **"Sửa"** button on group row
2. Modify fields as needed
3. Click **"Cập nhật"** to save

### Delete Group
1. Click **"Xóa"** button on group row
2. Confirm deletion in popup
3. Group is removed (cascades to fabrics)

## 🎨 Fabric Management

### View All Fabrics
- **Path:** Admin → Quản lý vải → Danh sách vải
- **Display:** Table with image, name, group, price, composition, status
- **Filter:** Use group buttons at top to filter by category

### Create New Fabric
1. Click **"Thêm vải mới"** button
2. Fill in required fields:
   - **Tên vải** (Fabric Name) - Required
   - **Nhóm vải** (Fabric Group) - Required dropdown
   - **Giá** (Price in VNĐ) - Required number
   - **Thành phần** (Composition) - Required
   - **Mô tả chi tiết** (Description) - Required textarea
3. Upload image (optional):
   - Click file input
   - Select image file (JPG, PNG)
   - Preview appears below
4. Click **"Thêm vải"** to save

### Edit Fabric
1. Click **"Sửa"** button on fabric row
2. Modify any fields:
   - Name, group, price, composition
   - Description
   - Availability status (checkbox)
3. To change image:
   - Select new image file
   - Leave blank to keep current image
4. Click **"Cập nhật"** to save

### Delete Fabric
1. Click **"Xóa"** button on fabric row
2. Confirm deletion in popup
3. Fabric is removed from all products

### Filter Fabrics
- Use group buttons at top of list
- Click group name to filter
- Click **"Tất cả"** to show all fabrics

## 🔗 Product-Fabric Association

### View Products
- **Path:** Admin → Quản lý vải → Gán vải cho sản phẩm
- **Display:** Paginated product list (10 per page)
- **Info:** Product image, name, category, price, fabric count

### Manage Fabrics for Product
1. Click **"Quản lý vải"** button on product row
2. Two-column interface appears:
   - **Left:** Assigned fabrics (currently linked)
   - **Right:** Available fabrics (not yet linked)

### Assign Fabric to Product
1. Find fabric in right column (Available)
2. Click **"+"** button next to fabric
3. Fabric moves to left column (Assigned)
4. Success message appears

### Remove Fabric from Product
1. Find fabric in left column (Assigned)
2. Click **trash icon** button
3. Confirm removal
4. Fabric moves back to right column

### Remove All Fabrics
1. Click **"Xóa tất cả"** button in left column
2. Confirm bulk removal
3. All fabrics are unassigned

## 💡 Tips & Tricks

### Image Upload
- Supported formats: JPG, PNG
- Max size: 5MB
- Images stored in: `/images/fabrics/`
- Preview shows before upload
- Default image used if none provided

### Pricing
- Enter prices in Vietnamese Đồng (VNĐ)
- Use step of 1000 for easier input
- Prices display with thousand separators

### Display Order
- Lower numbers appear first
- Use 1, 2, 3... for fabric groups
- Helps organize fabric groups logically

### Fabric Composition
- Examples:
  - "100% Len Merino"
  - "80% Wool, 20% Cashmere"
  - "65% Cotton, 35% Polyester"

### Availability Status
- Toggle checkbox when editing fabric
- Checked = Available (Có sẵn)
- Unchecked = Not available (Không có sẵn)

## ⚠️ Important Notes

### Cascading Deletes
- Deleting fabric group → deletes all fabrics in group
- Deleting fabric → removes from all products
- No undo available - use confirmation carefully

### Pagination
- Product list: 10 items per page
- Use pagination controls at bottom
- Navigate between pages easily

### Validation
- All required fields marked with *
- Form won't submit if validation fails
- Error messages appear in red

### Confirmation Dialogs
- Delete operations require confirmation
- Prevents accidental deletions
- Read carefully before confirming

## 🔍 Troubleshooting

### Image Not Uploading
- Check file format (JPG, PNG only)
- Verify file size < 5MB
- Ensure folder permissions are correct

### Fabric Not Appearing
- Check if fabric is marked as available
- Verify fabric group is assigned
- Refresh page to see updates

### Product Not Showing
- Check pagination (may be on another page)
- Verify product is not hidden
- Try searching in product list

## 📞 Support

For issues or questions:
1. Check error messages displayed
2. Review validation requirements
3. Verify user has Administrator role
4. Check browser console for errors

---
**Last Updated:** October 23, 2025
**Version:** 1.0

