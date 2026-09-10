using ElegantSuits.Domain.Entities;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IProductWriteRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Delete(Product product);
}
