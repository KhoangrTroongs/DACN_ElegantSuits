using ElegantSuits.Domain.Entities;

namespace ElegantSuits.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
