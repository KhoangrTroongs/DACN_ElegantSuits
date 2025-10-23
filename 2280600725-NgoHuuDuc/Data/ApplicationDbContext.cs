using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.IO;
using NgoHuuDuc_2280600725.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NgoHuuDuc_2280600725.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<FabricGroup> FabricGroups { get; set; }
        public DbSet<Fabric> Fabrics { get; set; }
        public DbSet<FabricProduct> FabricProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Đọc chuỗi kết nối từ file appsettings.json
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);

                // Bỏ qua cảnh báo về các thay đổi mô hình chưa được cập nhật vào database
                optionsBuilder.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đổi tên các bảng mặc định của Identity để dễ quản lý
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
            });
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Thiết lập quan hệ giữa Product và Category (1-nhiều)
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Thiết lập quan hệ giữa CartItem và Cart (1-nhiều)
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId);

            // Thiết lập quan hệ giữa Order và User (1-nhiều), khi xóa User thì UserId trong Order sẽ thành null
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Thiết lập quan hệ giữa OrderDetail và Order (1-nhiều)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            // Thiết lập quan hệ giữa OrderDetail và Product (1-nhiều)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);

            // Thiết lập quan hệ giữa ProductSize và Product (1-nhiều)
            builder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId);

            // Thiết lập quan hệ giữa ProductReview và Product (1-nhiều)
            builder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(pr => pr.ProductId);

            // Thiết lập quan hệ giữa ProductReview và User (1-nhiều)
            builder.Entity<ProductReview>()
                .HasOne(pr => pr.User)
                .WithMany()
                .HasForeignKey(pr => pr.UserId);

            // Thiết lập quan hệ giữa Fabric và FabricGroup (1-nhiều)
            builder.Entity<Fabric>()
                .HasOne(f => f.FabricGroup)
                .WithMany(fg => fg.Fabrics)
                .HasForeignKey(f => f.FabricGroupId);

            // Thiết lập quan hệ giữa FabricProduct và Fabric (1-nhiều)
            builder.Entity<FabricProduct>()
                .HasOne(fp => fp.Fabric)
                .WithMany(f => f.FabricProducts)
                .HasForeignKey(fp => fp.FabricId);

            // Thiết lập quan hệ giữa FabricProduct và Product (1-nhiều)
            builder.Entity<FabricProduct>()
                .HasOne(fp => fp.Product)
                .WithMany(p => p.FabricProducts)
                .HasForeignKey(fp => fp.ProductId);

            // Seed dữ liệu mẫu cho bảng Category
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Veston", Description = "Các loại veston xịn xịn" },
                new Category { Id = 2, Name = "Quần tây", Description = "Các loại quần tây tây - chất chơi người dơi" },
                new Category { Id = 3, Name = "Áo sơ mi", Description = "Áo sơ mi 2 trong 1" },
                new Category { Id = 4, Name = "Áo Gile", Description = "Các loại áo Gile, phù hợp nhiều mục đích" },
                new Category { Id = 5, Name = "Phụ Kiện", Description = "Các loại phụ kiện phù hợp cho từng sự kiện" }
            );


        }

        // Hàm seed role cho hệ thống, tạo các role nếu chưa có
        public async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Administrator", "User", "Staff", "Manager" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        // Hàm seed dữ liệu vải (Fabric) cho hệ thống
        public async Task SeedFabricDataAsync()
        {
            // Kiểm tra xem đã có dữ liệu vải chưa
            if (await FabricGroups.AnyAsync())
            {
                return; // Đã có dữ liệu, không cần seed lại
            }

            var seedDate = new DateTime(2024, 10, 23);

            // Tạo các nhóm vải
            var fabricGroups = new List<FabricGroup>
            {
                new FabricGroup { Name = "Len", Description = "Vải len cao cấp, ấm áp và bền", DisplayOrder = 1, CreatedAt = seedDate },
                new FabricGroup { Name = "Cotton", Description = "Vải cotton tự nhiên, thoáng khí", DisplayOrder = 2, CreatedAt = seedDate },
                new FabricGroup { Name = "Lụa", Description = "Vải lụa mềm mại, sang trọng", DisplayOrder = 3, CreatedAt = seedDate },
                new FabricGroup { Name = "Lanh", Description = "Vải lanh tự nhiên, mát mẻ", DisplayOrder = 4, CreatedAt = seedDate },
                new FabricGroup { Name = "Cashmere", Description = "Vải cashmere cao cấp, mềm mại", DisplayOrder = 5, CreatedAt = seedDate },
                new FabricGroup { Name = "Polyester", Description = "Vải polyester bền, dễ chăm sóc", DisplayOrder = 6, CreatedAt = seedDate },
                new FabricGroup { Name = "Denim", Description = "Vải denim bền bỉ, phong cách", DisplayOrder = 7, CreatedAt = seedDate },
                new FabricGroup { Name = "Kaki", Description = "Vải kaki thoáng khí, thoải mái", DisplayOrder = 8, CreatedAt = seedDate }
            };

            await FabricGroups.AddRangeAsync(fabricGroups);
            await SaveChangesAsync();

            // Reload fabric groups to get their IDs
            var lenGroup = await FabricGroups.FirstAsync(g => g.Name == "Len");
            var cottonGroup = await FabricGroups.FirstAsync(g => g.Name == "Cotton");
            var silkGroup = await FabricGroups.FirstAsync(g => g.Name == "Lụa");
            var linenGroup = await FabricGroups.FirstAsync(g => g.Name == "Lanh");
            var cashmereGroup = await FabricGroups.FirstAsync(g => g.Name == "Cashmere");
            var polyesterGroup = await FabricGroups.FirstAsync(g => g.Name == "Polyester");
            var denimGroup = await FabricGroups.FirstAsync(g => g.Name == "Denim");
            var khakiGroup = await FabricGroups.FirstAsync(g => g.Name == "Kaki");

            // Tạo các loại vải
            var fabrics = new List<Fabric>
            {
                // Len
                new Fabric { Name = "Len Merino Xanh Đen", Description = "Len Merino cao cấp màu xanh đen, phù hợp cho vest công sở", Composition = "100% Len Merino", FabricGroupId = lenGroup.Id, Price = 150000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Len Wool Xám Nhạt", Description = "Len wool mềm mại màu xám nhạt, lý tưởng cho vest casual", Composition = "100% Wool", FabricGroupId = lenGroup.Id, Price = 120000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Len Cashmere Blend Nâu", Description = "Hỗn hợp len và cashmere màu nâu, rất ấm áp", Composition = "80% Wool, 20% Cashmere", FabricGroupId = lenGroup.Id, Price = 200000, IsAvailable = true, CreatedAt = seedDate },

                // Cotton
                new Fabric { Name = "Cotton Trắng Tinh Khôi", Description = "Cotton 100% màu trắng, thoáng khí và thoải mái", Composition = "100% Cotton", FabricGroupId = cottonGroup.Id, Price = 80000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Cotton Xanh Dương", Description = "Cotton màu xanh dương, phù hợp cho áo sơ mi", Composition = "100% Cotton", FabricGroupId = cottonGroup.Id, Price = 85000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Cotton Blend Đỏ Đô", Description = "Hỗn hợp cotton và polyester màu đỏ đô", Composition = "65% Cotton, 35% Polyester", FabricGroupId = cottonGroup.Id, Price = 75000, IsAvailable = true, CreatedAt = seedDate },

                // Lụa
                new Fabric { Name = "Lụa Tơ Tằm Đen", Description = "Lụa tơ tằm nguyên chất màu đen, sang trọng và quý phái", Composition = "100% Silk", FabricGroupId = silkGroup.Id, Price = 250000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Lụa Blend Vàng Ấm", Description = "Hỗn hợp lụa và cotton màu vàng ấm", Composition = "70% Silk, 30% Cotton", FabricGroupId = silkGroup.Id, Price = 180000, IsAvailable = true, CreatedAt = seedDate },

                // Lanh
                new Fabric { Name = "Lanh Tự Nhiên Kem", Description = "Lanh 100% màu kem, mát mẻ và thoáng khí", Composition = "100% Linen", FabricGroupId = linenGroup.Id, Price = 95000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Lanh Xanh Rêu", Description = "Lanh màu xanh rêu, phù hợp cho quần tây mùa hè", Composition = "100% Linen", FabricGroupId = linenGroup.Id, Price = 100000, IsAvailable = true, CreatedAt = seedDate },

                // Cashmere
                new Fabric { Name = "Cashmere Xám Sáng", Description = "Cashmere 100% màu xám sáng, mềm mại và ấm áp", Composition = "100% Cashmere", FabricGroupId = cashmereGroup.Id, Price = 350000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Cashmere Đen Tuyền", Description = "Cashmere 100% màu đen tuyền, cao cấp và sang trọng", Composition = "100% Cashmere", FabricGroupId = cashmereGroup.Id, Price = 380000, IsAvailable = true, CreatedAt = seedDate },

                // Polyester
                new Fabric { Name = "Polyester Xanh Navy", Description = "Polyester bền màu xanh navy, dễ chăm sóc", Composition = "100% Polyester", FabricGroupId = polyesterGroup.Id, Price = 60000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Polyester Trắng Sáng", Description = "Polyester màu trắng sáng, không nhăn", Composition = "100% Polyester", FabricGroupId = polyesterGroup.Id, Price = 55000, IsAvailable = true, CreatedAt = seedDate },

                // Denim
                new Fabric { Name = "Denim Xanh Đậm", Description = "Denim xanh đậm bền bỉ, phong cách cổ điển", Composition = "100% Cotton Denim", FabricGroupId = denimGroup.Id, Price = 90000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Denim Xanh Nhạt", Description = "Denim xanh nhạt, thoáng khí và thoải mái", Composition = "100% Cotton Denim", FabricGroupId = denimGroup.Id, Price = 85000, IsAvailable = true, CreatedAt = seedDate },

                // Kaki
                new Fabric { Name = "Kaki Kem Nhạt", Description = "Kaki màu kem nhạt, thoáng khí và thoải mái", Composition = "100% Cotton Kaki", FabricGroupId = khakiGroup.Id, Price = 75000, IsAvailable = true, CreatedAt = seedDate },
                new Fabric { Name = "Kaki Nâu Nhạt", Description = "Kaki màu nâu nhạt, phù hợp cho quần tây casual", Composition = "100% Cotton Kaki", FabricGroupId = khakiGroup.Id, Price = 80000, IsAvailable = true, CreatedAt = seedDate }
            };

            await Fabrics.AddRangeAsync(fabrics);
            await SaveChangesAsync();

            // Gán vải cho các sản phẩm một cách ngẫu nhiên
            var products = await Products.ToListAsync();
            var random = new Random();

            foreach (var product in products)
            {
                // Chọn 1-3 loại vải ngẫu nhiên cho mỗi sản phẩm
                int fabricCount = random.Next(1, 4);
                var selectedFabrics = fabrics.OrderBy(x => random.Next()).Take(fabricCount).ToList();

                foreach (var fabric in selectedFabrics)
                {
                    var fabricProduct = new FabricProduct
                    {
                        FabricId = fabric.Id,
                        ProductId = product.Id,
                        IsAvailable = true,
                        CreatedAt = seedDate
                    };

                    await FabricProducts.AddAsync(fabricProduct);
                }
            }

            await SaveChangesAsync();
        }
    }
}
