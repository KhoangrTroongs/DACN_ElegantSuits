# Custom Suit Design Feature - Quick Start Guide

## 🚀 Getting Started

### Access the Feature
1. **Navigate to the Custom Design Page:**
   - Click on "Thiết Kế Vest" (Custom Suit Design) in the main navigation menu
   - Or visit: `https://localhost:7001/CustomDesign`

### Feature Workflow

#### Step 1: Browse Fabric Groups
- View all available fabric groups (Wool, Cotton, Silk, Linen, Cashmere, etc.)
- Click on any group to see available fabrics
- Each fabric shows composition and pricing

#### Step 2: View Fabric Details
- Click on a fabric to see detailed information
- View fabric composition, price, and availability
- Navigate back to browse other fabrics

#### Step 3: Select a Product
- Click "Chọn Sản Phẩm" (Select Product) to view available products
- Browse products available for custom design
- Click on a product to start customization

#### Step 4: Design Your Custom Suit
- View product preview on the left sidebar
- Browse fabrics organized by group in accordion layout
- Select one or more fabrics for your custom design
- View selected fabrics summary with total pricing
- Click "Thêm Vào Giỏ Hàng" (Add to Cart) to proceed

## 📊 Database Information

### Fabric Groups (8 Total)
1. **Len** (Wool) - Premium wool fabrics
2. **Cotton** - Cotton blends
3. **Lụa** (Silk) - Silk fabrics
4. **Lanh** (Linen) - Linen fabrics
5. **Cashmere** - Luxury cashmere
6. **Polyester** - Synthetic blends
7. **Denim** - Denim fabrics
8. **Kaki** (Khaki) - Khaki fabrics

### Sample Fabrics (18 Total)
- Each group contains 2-3 sample fabrics
- All fabrics have realistic compositions and pricing
- Randomly assigned to existing products (1-3 per product)

## 🔌 API Endpoints

### Fabric Groups
- `GET /api/fabrics/groups` - Get all fabric groups
- `GET /api/fabrics/groups/{id}` - Get specific group
- `POST /api/fabrics/groups` - Create new group (Admin)
- `PUT /api/fabrics/groups/{id}` - Update group (Admin)
- `DELETE /api/fabrics/groups/{id}` - Delete group (Admin)

### Fabrics
- `GET /api/fabrics` - Get all fabrics
- `GET /api/fabrics/{id}` - Get specific fabric
- `GET /api/fabrics/group/{groupId}` - Get fabrics by group
- `GET /api/fabrics/product/{productId}` - Get fabrics for product
- `POST /api/fabrics` - Create fabric (Admin)
- `PUT /api/fabrics/{id}` - Update fabric (Admin)
- `DELETE /api/fabrics/{id}` - Delete fabric (Admin)

### Product-Fabric Association
- `POST /api/fabrics/product/{productId}/fabric/{fabricId}` - Add fabric to product
- `DELETE /api/fabrics/product/{productId}/fabric/{fabricId}` - Remove fabric from product

## 🎨 UI Components

### Fabric Groups Page
- Card-based grid layout
- Responsive design (mobile-friendly)
- Group name, description, and fabric count
- Click to view fabrics in group

### Fabric Group Details
- Accordion-style fabric listing
- Fabric images, composition, and pricing
- Quick view details
- Navigation breadcrumbs

### Product Selection
- Product grid with images
- Product name and price
- Stock availability
- Click to customize

### Design Interface
- **Left Sidebar (Sticky):**
  - Product preview image
  - Product details (name, price, quantity)
  - Stays visible while scrolling

- **Main Content:**
  - Accordion-organized fabric groups
  - Fabric selection checkboxes
  - Real-time selection summary
  - Total pricing calculation

## 🔧 Technical Stack

- **Framework:** ASP.NET Core 9.0 MVC
- **Database:** SQL Server with EF Core 9.0.3
- **Frontend:** Bootstrap 5, Razor Views
- **API:** RESTful with JSON responses
- **Authentication:** ASP.NET Identity
- **Async:** Full async/await support

## 📝 Key Features

✅ Fabric catalog with hierarchical grouping
✅ Product-fabric associations (many-to-many)
✅ Real-time fabric selection and pricing
✅ Responsive design for all devices
✅ Vietnamese localization
✅ RESTful API for integration
✅ Admin management capabilities
✅ Sticky product preview
✅ Accordion-based navigation
✅ Dynamic pricing calculation

## 🐛 Troubleshooting

### Fabrics Not Showing
- Ensure database migration has been applied
- Check that fabric seeding completed on startup
- Verify database connection string in appsettings.json

### Images Not Loading
- Check `wwwroot/images/catolog` folder exists
- Verify image paths in database
- Ensure proper file permissions

### API Endpoints Not Working
- Verify authentication/authorization if required
- Check request format (JSON)
- Review error messages in browser console

## 📞 Support

For issues or questions:
1. Check the IMPLEMENTATION_COMPLETE.md for detailed technical info
2. Review API documentation in Swagger UI
3. Check browser console for JavaScript errors
4. Review application logs for server-side errors

---
**Last Updated:** October 23, 2025
**Version:** 1.0

