using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Products.Contracts;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly ApplicationDbContext _context;

    public ProductReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetProductsByCategoryAsync(
        int? categoryId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking();

        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Model3DUrl = p.Model3DUrl,
                Quantity = p.Quantity,
                IsHidden = p.IsHidden,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                LinearCode = p.LinearCode,
                ProfitMargin = p.ProfitMargin,
                AverageRating = p.ProductReviews.Any() ? p.ProductReviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = p.ProductReviews.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedList<ProductResponse>> GetPagedProductsAsync(
        int? categoryId,
        int pageIndex,
        int pageSize,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking();

        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var validPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var validPageSize = pageSize < 1 ? 10 : pageSize;

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((validPageIndex - 1) * validPageSize)
            .Take(validPageSize)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Model3DUrl = p.Model3DUrl,
                Quantity = p.Quantity,
                IsHidden = p.IsHidden,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                LinearCode = p.LinearCode,
                ProfitMargin = p.ProfitMargin,
                AverageRating = p.ProductReviews.Any() ? p.ProductReviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = p.ProductReviews.Count
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductResponse>(items, totalCount, validPageIndex, validPageSize);
    }

    public async Task<ProductResponse?> GetProductByIdAsync(
        int id,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking().Where(p => p.Id == id);

        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        return await query
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Model3DUrl = p.Model3DUrl,
                Quantity = p.Quantity,
                IsHidden = p.IsHidden,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                LinearCode = p.LinearCode,
                ProfitMargin = p.ProfitMargin,
                AverageRating = p.ProductReviews.Any() ? p.ProductReviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = p.ProductReviews.Count
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedList<ProductResponse>> SearchProductsAsync(
        string keyword,
        int pageIndex,
        int pageSize,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking();

        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var validPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var validPageSize = pageSize < 1 ? 10 : pageSize;

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((validPageIndex - 1) * validPageSize)
            .Take(validPageSize)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Model3DUrl = p.Model3DUrl,
                Quantity = p.Quantity,
                IsHidden = p.IsHidden,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                LinearCode = p.LinearCode,
                ProfitMargin = p.ProfitMargin,
                AverageRating = p.ProductReviews.Any() ? p.ProductReviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = p.ProductReviews.Count
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductResponse>(items, totalCount, validPageIndex, validPageSize);
    }
}
