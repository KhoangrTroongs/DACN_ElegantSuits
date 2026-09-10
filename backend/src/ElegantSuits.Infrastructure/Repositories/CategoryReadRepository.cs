using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<string?> GetNameByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
