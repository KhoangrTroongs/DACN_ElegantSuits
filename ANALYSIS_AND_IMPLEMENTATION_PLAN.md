# DACS Elegant Suits - Custom Suit Design Feature
## Phase 1: Project Analysis & Phase 2: Implementation Plan

---

## PHASE 1: PROJECT ANALYSIS SUMMARY

### 1. Architecture & Patterns ✓
**Framework**: ASP.NET Core 9.0 MVC with Entity Framework Core 9.0.3
**Database**: SQL Server with Identity Framework
**Architecture Pattern**: Clean Layered Architecture
- **Controllers Layer**: MVC Controllers + API Controllers (REST)
- **Service Layer**: Business logic (IProductService, ICategoryService, etc.)
- **Repository Layer**: Data access (IProductRepository, ICategoryRepository)
- **Models/DTOs**: Domain entities and Data Transfer Objects
- **Views**: Razor views with Bootstrap 5 responsive design

**Key Patterns**:
- Dependency Injection via constructor
- Repository Pattern with Entity Framework Core
- Service-based business logic
- DTO pattern for API communication
- Async/await throughout

### 2. Core Domain Entities ✓
**Product**: Name, Description, Price, ImageUrl, Model3DUrl, Quantity, IsHidden, CategoryId
**Category**: Id, Name, Description (5 categories: Veston, Quần tây, Áo sơ mi, Áo Gile, Phụ Kiện)
**ApplicationUser**: Extends IdentityUser with FullName, DateOfBirth, Address, AvatarUrl, Gender
**Cart/CartItem**: Shopping cart functionality
**Order/OrderDetail**: Order management with OrderStatus enum
**ProductSize**: Product size variants
**ProductReview**: Product reviews with ratings

**Relationships**:
- Product → Category (Many-to-One)
- Product → ProductSize (One-to-Many)
- Product → ProductReview (One-to-Many)
- Order → OrderDetail (One-to-Many)
- Order → ApplicationUser (Many-to-One, SetNull on delete)

### 3. Current Features & UI Patterns ✓
**Product Management**:
- Product CRUD operations (Admin only)
- Product catalog with filtering by category
- Product search functionality
- Pagination support
- Product visibility toggle (IsHidden)
- Stock management

**UI Patterns**:
- Bootstrap 5 responsive design
- Elegant theme with custom CSS (elegant-theme.css)
- Vietnamese language throughout UI
- Dashboard layout for admin
- Card-based product display
- Category sidebar filtering

**Authentication/Authorization**:
- ASP.NET Identity with roles: Administrator, User, Staff, Manager
- Role-based access control
- Session-based authentication
- Cookie-based persistence (7-day expiration)

### 4. Technology Stack Confirmation ✓
- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0.3
- **Database**: SQL Server
- **Frontend**: Bootstrap 5, jQuery, Select2
- **Image Storage**: wwwroot/images/ (products, users, catolog folders)
- **Authentication**: ASP.NET Identity + JWT support
- **API**: RESTful API with Swagger/OpenAPI documentation

---

## PHASE 2: IMPLEMENTATION PLAN

### Database Schema Changes

**New Entity: Fabric**
```
- Id (int, PK)
- Name (string, required)
- Description (string)
- Composition (string) - e.g., "100% Wool", "80% Wool, 20% Silk"
- ImageUrl (string)
- Price (decimal) - premium price for this fabric
- FabricGroupId (int, FK)
- IsAvailable (bool)
- CreatedAt (DateTime)
```

**New Entity: FabricGroup**
```
- Id (int, PK)
- Name (string, required) - e.g., "Wool", "Cotton", "Silk"
- Description (string)
- DisplayOrder (int)
```

**New Entity: FabricProduct** (Junction table)
```
- Id (int, PK)
- FabricId (int, FK)
- ProductId (int, FK)
- IsAvailable (bool)
```

**Relationships**:
- FabricGroup → Fabric (One-to-Many)
- Fabric → FabricProduct (One-to-Many)
- Product → FabricProduct (One-to-Many)

### Implementation Tasks

#### Phase 2.1: Database & Models
- [ ] Create Fabric entity class
- [ ] Create FabricGroup entity class
- [ ] Create FabricProduct junction entity
- [ ] Update ApplicationDbContext with new DbSets and relationships
- [ ] Create EF Core migration
- [ ] Seed initial fabric groups and fabrics

#### Phase 2.2: Repository & Service Layer
- [ ] Create IFabricRepository interface
- [ ] Create EFFabricRepository implementation
- [ ] Create IFabricService interface
- [ ] Create FabricService implementation
- [ ] Add methods: GetAllFabricGroups, GetFabricsByGroup, GetFabricById, etc.

#### Phase 2.3: DTOs
- [ ] Create FabricDTO, CreateFabricDTO, UpdateFabricDTO
- [ ] Create FabricGroupDTO
- [ ] Create FabricProductDTO

#### Phase 2.4: Controllers & Views
- [ ] Create CustomDesignController (MVC)
- [ ] Create API FabricsController (REST)
- [ ] Create views: Index (fabric groups), FabricGroup (fabric selection), FabricDetail
- [ ] Implement responsive grid layout for fabric display
- [ ] Add fabric image gallery

#### Phase 2.5: Navigation & Routing
- [ ] Add "Thiết Kế Vest" menu item to _Layout.cshtml
- [ ] Configure routes for custom design workflow
- [ ] Add authorization checks (authenticated users only)

#### Phase 2.6: Image Management
- [ ] Utilize wwwroot/images/catolog for fabric images
- [ ] Implement image path resolution
- [ ] Support responsive image loading

#### Phase 2.7: Testing & Refinement
- [ ] Test fabric catalog display
- [ ] Test fabric filtering and search
- [ ] Test responsive design on mobile
- [ ] Verify authorization and authentication

---

## File Structure Overview

```
2280600725-NgoHuuDuc/
├── Models/
│   ├── Fabric.cs (NEW)
│   ├── FabricGroup.cs (NEW)
│   ├── FabricProduct.cs (NEW)
│   └── [existing models]
├── Services/
│   ├── Interfaces/
│   │   ├── IFabricService.cs (NEW)
│   │   └── [existing interfaces]
│   ├── FabricService.cs (NEW)
│   └── [existing services]
├── Responsitories/
│   ├── IFabricRepository.cs (NEW)
│   ├── EFFabricRepository.cs (NEW)
│   └── [existing repositories]
├── Controllers/
│   ├── CustomDesignController.cs (NEW)
│   ├── API/FabricsController.cs (NEW)
│   └── [existing controllers]
├── DTOs/
│   ├── FabricDTO.cs (NEW)
│   └── [existing DTOs]
├── Views/
│   ├── CustomDesign/ (NEW)
│   │   ├── Index.cshtml
│   │   ├── FabricGroup.cshtml
│   │   └── FabricDetail.cshtml
│   └── [existing views]
├── Data/
│   ├── ApplicationDbContext.cs (MODIFIED)
│   └── Migrations/ (NEW migration)
└── wwwroot/
    └── images/
        └── catolog/ (existing, will use for fabric images)
```

---

## Next Steps

1. **Proceed with Phase 2.1**: Create database entities and migration
2. **Proceed with Phase 2.2**: Implement repository and service layers
3. **Proceed with Phase 2.3-2.4**: Create controllers, views, and DTOs
4. **Proceed with Phase 2.5-2.6**: Add navigation and image management
5. **Proceed with Phase 2.7**: Testing and refinement

All code will follow existing patterns:
- Vietnamese UI labels, English code
- Async/await patterns
- Dependency injection
- Repository pattern
- Service layer abstraction
- Bootstrap 5 responsive design

