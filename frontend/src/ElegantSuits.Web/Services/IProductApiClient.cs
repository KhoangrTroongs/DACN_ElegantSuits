using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IProductApiClient
{
    Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(int? categoryId = null, CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductViewModel>?> GetPagedProductsAsync(int? categoryId = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ProductViewModel?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductViewModel>?> SearchProductsAsync(string keyword, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ResponseDTO<ProductViewModel>> CreateProductAsync(CreateProductViewModel model, CancellationToken cancellationToken = default);
    Task<ResponseDTO<ProductViewModel>> UpdateProductAsync(int id, UpdateProductViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
