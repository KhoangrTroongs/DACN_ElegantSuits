using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSize> ProductSizes => Set<ProductSize>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<FabricGroup> FabricGroups => Set<FabricGroup>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();
    public DbSet<FabricProduct> FabricProducts => Set<FabricProduct>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

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

        // Thiết lập quan hệ giữa Order và User (1-nhiều)
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
    }
}
