using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly ApplicationDbContext _context;

    public ProductWriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra xem sản phẩm đã có trong đơn hàng nào chưa
        var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == product.Id, cancellationToken);
        if (hasOrders)
        {
            // Nếu đã phát sinh đơn hàng, chuyển sang Soft Delete (ẩn sản phẩm) để bảo vệ lịch sử hóa đơn
            product.IsHidden = true;
            _context.Products.Update(product);
            return;
        }

        // 2. Nếu chưa phát sinh đơn hàng, xóa sạch các ràng buộc phụ thuộc trước khi xóa sản phẩm
        var sizes = await _context.ProductSizes.Where(ps => ps.ProductId == product.Id).ToListAsync(cancellationToken);
        if (sizes.Any()) _context.ProductSizes.RemoveRange(sizes);

        var cartItems = await _context.CartItems.Where(ci => ci.ProductId == product.Id).ToListAsync(cancellationToken);
        if (cartItems.Any()) _context.CartItems.RemoveRange(cartItems);

        var reviews = await _context.ProductReviews.Where(pr => pr.ProductId == product.Id).ToListAsync(cancellationToken);
        if (reviews.Any()) _context.ProductReviews.RemoveRange(reviews);

        _context.Products.Remove(product);
    }

    public void Delete(Product product)
    {
        DeleteAsync(product).GetAwaiter().GetResult();
    }
}
