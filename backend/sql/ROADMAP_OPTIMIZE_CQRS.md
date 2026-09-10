# Elegant Suits - Lo trinh toi uu solution va tach CQRS

## 1. Muc tieu

Tai cau truc solution theo huong:

- Backend API doc lap voi frontend.
- CQRS theo tung feature, khong gom tat ca logic vao service lon.
- Giu nguyen database va API contract trong giai doan dau de giam rui ro.
- Co the di chuyen dan, khong rewrite toan bo mot lan.
- Domain va application khong phu thuoc vao ASP.NET MVC, Razor, EF Core hoac frontend.

Khuyen nghi kien truc: **Modular Monolith + CQRS truoc, microservices sau neu that su can**.

Khong nen tach database read/write hoac chia thanh microservices ngay luc nay. He thong hien tai chua co boundary nghiep vu du on dinh de viec do mang lai loi ich.

---

## 2. Hien trang da xac nhan

Solution hien tai dang la mot ASP.NET Core Web project duy nhat:

```text
2280600725-NgoHuuDuc
+-- Controllers/              MVC controller cho Razor UI
+-- Controllers/API/          API controller cho mobile/web client
+-- Views/                    Razor frontend
+-- wwwroot/                  static files, images, javascript, css
+-- Models/                   entity va view model dang dung chung
+-- DTOs/                     DTO dang gom theo project, chua theo feature
+-- Services/                 application service, mapping, file upload, nghiep vu
+-- Responsitories/           EF repository
+-- Data/                     ApplicationDbContext
+-- Program.cs                DI, middleware, authentication, seed, pipeline
+-- 2280600725-NgoHuuDuc.csproj
```

Cac van de chinh:

1. Razor MVC va API dang deploy chung mot project.
2. `ProductController` truy cap repository/service va xu ly nhieu nghiep vu ngay trong controller.
3. `ProductService` vua query, command, map DTO, vua luu/xoa file.
4. API controller lap lai try/catch, kiem tra user, response va xu ly loi.
5. `ApplicationDbContext` dang chua ca persistence config va seed du lieu.
6. `Program.cs` dang chua qua nhieu cau hinh, seed tai khoan va khoi tao database.
7. `AllowAnyOrigin`, HTTP khong redirect va credential seed cung code can duoc xem lai truoc production.
8. Repository dang goi `SaveChangesAsync()` trong tung method, lam kho kiem soat transaction cua command.

---

## 3. Kien truc muc tieu

```text
ElegantSuits.sln
+-- src/
|   +-- ElegantSuits.Api/
|   +-- ElegantSuits.Application/
|   +-- ElegantSuits.Domain/
|   +-- ElegantSuits.Infrastructure/
|
+-- tests/
|   +-- ElegantSuits.Application.Tests/
|   +-- ElegantSuits.Api.Tests/
|
+-- frontend/
    +-- elegant-suits-web/       # React/Vue neu chuyen SPA
    # hoac elegant-suits-flutter/ # neu Flutter la frontend chinh
```

### 3.1 Dependency rule

```text
ElegantSuits.Api           -> Application, Infrastructure
ElegantSuits.Infrastructure -> Application, Domain
ElegantSuits.Application   -> Domain
ElegantSuits.Domain        -> khong phu thuoc project nao
```

Khong cho phep:

- Domain tham chieu EF Core, ASP.NET hoac DTO cua API.
- Application tham chieu `IWebHostEnvironment`, `HttpContext`, `Controller`.
- Controller truy cap `ApplicationDbContext` truc tiep.
- Frontend tham chieu entity database.
- Infrastructure lam ro business rule cua tung use case.

---

## 4. Trach nhiem cua tung project

### ElegantSuits.Domain

Chua:

- Entity: `Product`, `Category`, `Cart`, `Order`, `Coupon`, `Fabric`.
- Value object, enum, domain exception.
- Business rule that su thuoc ve domain.
- Domain event neu can.

Khong chua DTO request/response, EF configuration hoac controller.

### ElegantSuits.Application

Chua use case theo feature:

```text
Products/
+-- Commands/
|   +-- CreateProduct/
|   +-- UpdateProduct/
|   +-- DeleteProduct/
|   +-- ChangeProductVisibility/
+-- Queries/
    +-- GetProducts/
    +-- GetProductById/
    +-- SearchProducts/
```

Moi use case nen co:

```text
CreateProduct/
+-- CreateProductCommand.cs
+-- CreateProductHandler.cs
+-- CreateProductValidator.cs
+-- CreateProductResult.cs
```

Application chi phu thuoc vao interface, vi du:

- `IProductReadRepository`
- `IProductWriteRepository`
- `IFileStorage`
- `ICurrentUser`
- `IUnitOfWork`
- `IDateTimeProvider`

### ElegantSuits.Infrastructure

Chua implementation:

```text
Persistence/
+-- ApplicationDbContext.cs
+-- Configurations/
+-- Migrations/

Repositories/
+-- ProductReadRepository.cs
+-- ProductWriteRepository.cs

Identity/
FileStorage/
Payments/
Email/
```

EF Core, Identity, file system, MoMo, VNPay va email deu nam o day.

### ElegantSuits.Api

Chua:

- HTTP endpoint.
- Authentication/authorization.
- Model binding.
- Swagger/OpenAPI.
- CORS.
- Middleware xu ly exception.
- SignalR endpoint.

Controller chi nen nhan request, goi `ISender`, roi tra HTTP response.

---

## 5. Nguyen tac CQRS

### Query

Query chi doc, khong thay doi state:

```text
GET /api/products
    -> GetProductsQuery
    -> GetProductsQueryHandler

GET /api/products/{id}
    -> GetProductByIdQuery
    -> GetProductByIdQueryHandler

GET /api/products/search
    -> SearchProductsQuery
    -> SearchProductsQueryHandler
```

Query nen:

- Dung `AsNoTracking()`.
- Projection truc tiep sang response DTO.
- Khong tra entity EF ra API.
- Co pagination, filter va sort ro rang.

### Command

Command lam thay doi state:

```text
POST   /api/products       -> CreateProductCommand
PUT    /api/products/{id}  -> UpdateProductCommand
DELETE /api/products/{id}  -> DeleteProductCommand
```

Command nen:

- Validate input truoc khi xu ly.
- Kiem tra authorization trong application policy/use case khi can.
- Dung transaction cho thay doi nhieu bang.
- Goi `SaveChangesAsync()` mot lan o cuoi use case.
- Tra ve id/result, khong tra entity EF.

---

## 6. Lo trinh migration de xuat

## Giai doan 0 - Bao ve hien trang

Muc tieu: co diem bat dau an toan truoc khi refactor.

Checklist:

- [ ] Backup database.
- [ ] Chay `dotnet build` va ghi lai loi hien tai.
- [ ] Xac dinh API endpoint dang duoc Flutter/frontend su dung.
- [ ] Export Swagger/OpenAPI hien tai lam contract tham chieu.
- [ ] Ghi lai cac luong chinh: login, product, cart, order, payment.
- [ ] Them test toi thieu cho product list, create product, cart va order.
- [ ] Khong doi URL API va format response trong buoc nay.

## Giai doan 1 - Don dep boundary trong project hien tai

Chua tao project moi ngay. Tranh rui ro di chuyen khi boundary chua ro.

1. Tach API logic khoi `Controllers/API` vao cac use case.
2. Tach `ProductService` thanh cac query/command rieng.
3. Chuyen DTO theo feature:

```text
DTOs/ProductDTO.cs
-> Application/Products/Contracts/ProductResponse.cs
-> Application/Products/Commands/CreateProduct/CreateProductRequest.cs
-> Application/Products/Commands/UpdateProduct/UpdateProductRequest.cs
```

4. Dua upload file vao `IFileStorage`.
5. Dua mapping vao projection/mapper rieng.
6. Tao global exception middleware de bo try/catch lap lai trong controller.
7. Chuyen validation sang FluentValidation hoac validator rieng.

## Giai doan 2 - Tach project

Tao 4 project:

```bash
dotnet new webapi -n ElegantSuits.Api
dotnet new classlib -n ElegantSuits.Application
dotnet new classlib -n ElegantSuits.Domain
dotnet new classlib -n ElegantSuits.Infrastructure
```

Sau do:

- Di chuyen entity va enum vao Domain.
- Di chuyen interface use case/repository vao Application.
- Di chuyen EF, Identity, repository implementation vao Infrastructure.
- Di chuyen API controller va Program vao Api.
- Them project vao solution.
- Chay build sau moi lan di chuyen mot feature.

## Giai doan 3 - Migration theo feature

Thu tu khuyen nghi:

1. Product.
2. Category.
3. Fabric.
4. Cart.
5. Coupon.
6. Order.
7. Payment.
8. User/Identity.
9. Statistics.

Moi feature chi xem la hoan thanh khi co:

- [ ] Request/response contract.
- [ ] Query/command.
- [ ] Handler.
- [ ] Validator.
- [ ] Repository interface.
- [ ] Infrastructure implementation.
- [ ] Controller endpoint.
- [ ] Unit test handler.
- [ ] Integration test endpoint.
- [ ] Swagger duoc cap nhat.

## Giai doan 4 - Tach frontend

Neu dung React/Vue:

```text
frontend/elegant-suits-web
+-- src/api/
+-- src/features/products/
+-- src/features/cart/
+-- src/features/orders/
+-- src/components/
+-- src/pages/
```

Neu dung Flutter:

```text
frontend/elegant-suits-flutter
+-- lib/core/network/
+-- lib/features/products/
+-- lib/features/cart/
+-- lib/features/orders/
```

Frontend chi giao tiep qua API. Khong copy entity EF vao frontend; chi tao model tu API contract.

Sau khi frontend moi chay on dinh:

- [ ] Chuyen trang public khoi Razor neu khong con dung.
- [ ] Giữ Razor Admin tam thoi neu can.
- [ ] Xoa MVC controller/view khong con su dung.
- [ ] Xoa CORS policy cu va gioi han origin theo moi truong.

---

## 7. Mau flow cho Product

```text
ProductsController
    |
    +-- sender.Send(new GetProductsQuery(...))
              |
              +-- GetProductsQueryHandler
                        |
                        +-- IProductReadRepository
                                  |
                                  +-- EF projection -> ProductResponse
```

```text
ProductsController
    |
    +-- sender.Send(new CreateProductCommand(...))
              |
              +-- CreateProductCommandHandler
                    |
                    +-- ICategoryReadRepository
                    +-- IFileStorage
                    +-- IProductWriteRepository
                    +-- IUnitOfWork
```

Controller khong nen:

- Tao entity Product truc tiep.
- Goi `SaveChangesAsync()`.
- Doc file system.
- Tu viet business rule.
- Truy cap `User` de nhay vao nhieu nhanh logic phuc tap.

---

## 8. Chuan hoa response va loi

Nen dung mot format thong nhat:

```json
{
  "data": {},
  "errors": [],
  "message": null,
  "traceId": "..."
}
```

Quy uoc HTTP:

- `200 OK`: query/update thanh cong.
- `201 Created`: tao moi thanh cong.
- `204 No Content`: xoa thanh cong neu khong can body.
- `400 Bad Request`: request sai hoac business validation fail.
- `401 Unauthorized`: chua dang nhap.
- `403 Forbidden`: khong du quyen.
- `404 Not Found`: resource khong ton tai.
- `409 Conflict`: xung dot nghiep vu.
- `500 Internal Server Error`: loi khong xu ly duoc.

Khong tra exception message noi bo truc tiep cho client trong production.

---

## 9. Viec can sua truoc production

- [ ] Thay `AllowAnyOrigin()` bang danh sach origin theo environment.
- [ ] Bat HTTPS trong production.
- [ ] Dua secret, JWT key, OAuth secret vao User Secrets/Key Vault/environment variables.
- [ ] Xoa password admin hard-code khoi `Program.cs`.
- [ ] Dung EF migration thay cho `EnsureCreatedAsync()`.
- [ ] Tach database seeding ra `DatabaseSeeder`.
- [ ] Khong log token va thong tin nhay cam.
- [ ] Gioi han kich thuoc va loai file upload o application service.
- [ ] Dung ten `Repositories` thay cho `Responsitories` khi tao code moi.
- [ ] Bo sung cancellation token cho I/O va EF query.
- [ ] Thiet lap health check, structured logging va request correlation id.

---

## 10. Quy tac code sau khi refactor

1. Moi handler chi phuc vu mot use case.
2. Moi class co mot ly do thay doi.
3. Khong tao service co hang chuc overload gan giong nhau.
4. Query khong duoc sua du lieu.
5. Command khong tra entity EF.
6. Khong dung `ViewBag` trong API/application layer.
7. Khong de application phu thuoc vao `HttpContext`.
8. Khong de frontend phu thuoc vao database model.
9. Moi query phai co gioi han page size hop ly.
10. Moi thay doi boundary phai co test hoac contract test tuong ung.

---

## 11. Definition of Done

Mot feature duoc xem la da migrate khi:

- API controller khong con logic truy van/ghi DB.
- Handler xu ly tron ven mot use case.
- Application khong tham chieu EF Core hoac ASP.NET.
- Infrastructure la noi duy nhat biet EF Core.
- Response khong expose entity/navigation property.
- Co unit test cho handler va integration test cho endpoint chinh.
- Swagger va frontend client dung dung contract.
- `dotnet build` pass.
- Database migration va rollback strategy da duoc xac dinh.
- Khong con endpoint duplicate giua MVC va API ma khong co ly do ro rang.

---

## 12. Buoc tiep theo cu the

Bat dau bang Product, khong sua toan bo solution cung luc:

1. Tao `ProductResponse`, `GetProductsQuery`, `GetProductByIdQuery`.
2. Tao handler cho hai query nay.
3. Cho `ProductsController` goi query qua MediatR.
4. Viet test cho list/detail va quyen xem san pham an.
5. Tiep tuc voi `CreateProductCommand`.
6. Khi Product da on dinh, dung no lam mau de migrate Category va Cart.

Muc tieu cua buoc dau tien khong phai la tao nhieu folder, ma la chung minh boundary sau:

```text
HTTP -> Controller -> Query/Command Handler -> Abstraction -> Infrastructure -> Database
```
