using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Products.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IProductReadRepository
{
    Task<IReadOnlyList<ProductResponse>> GetProductsByCategoryAsync(int? categoryId, bool includeHidden = false, CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductResponse>> GetPagedProductsAsync(int? categoryId, int pageIndex, int pageSize, bool includeHidden = false, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetProductByIdAsync(int id, bool includeHidden = false, CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductResponse>> SearchProductsAsync(string keyword, int pageIndex, int pageSize, bool includeHidden = false, CancellationToken cancellationToken = default);
}
