namespace ElegantSuits.Application.Common.Interfaces;

public interface ICategoryReadRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> GetNameByIdAsync(int id, CancellationToken cancellationToken = default);
}
