using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class FabricRepository : IFabricRepository
{
    private readonly ApplicationDbContext _context;

    public FabricRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FabricGroupDTO>> GetAllFabricGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FabricGroups
            .AsNoTracking()
            .OrderBy(fg => fg.DisplayOrder)
            .Select(fg => new FabricGroupDTO
            {
                Id = fg.Id,
                Name = fg.Name,
                Description = fg.Description,
                DisplayOrder = fg.DisplayOrder,
                CreatedAt = fg.CreatedAt,
                Fabrics = fg.Fabrics.Select(f => new FabricDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Composition = f.Composition,
                    ImageUrl = f.ImageUrl,
                    Price = f.Price,
                    FabricGroupId = f.FabricGroupId,
                    FabricGroupName = fg.Name,
                    IsAvailable = f.IsAvailable,
                    CreatedAt = f.CreatedAt
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FabricGroupDTO?> GetFabricGroupByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FabricGroups
            .AsNoTracking()
            .Where(fg => fg.Id == id)
            .Select(fg => new FabricGroupDTO
            {
                Id = fg.Id,
                Name = fg.Name,
                Description = fg.Description,
                DisplayOrder = fg.DisplayOrder,
                CreatedAt = fg.CreatedAt,
                Fabrics = fg.Fabrics.Select(f => new FabricDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Composition = f.Composition,
                    ImageUrl = f.ImageUrl,
                    Price = f.Price,
                    FabricGroupId = f.FabricGroupId,
                    FabricGroupName = fg.Name,
                    IsAvailable = f.IsAvailable,
                    CreatedAt = f.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FabricDTO>> GetAllFabricsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Fabrics
            .AsNoTracking()
            .Include(f => f.FabricGroup)
            .Select(f => new FabricDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Composition = f.Composition,
                ImageUrl = f.ImageUrl,
                Price = f.Price,
                FabricGroupId = f.FabricGroupId,
                FabricGroupName = f.FabricGroup != null ? f.FabricGroup.Name : null,
                IsAvailable = f.IsAvailable,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FabricDTO?> GetFabricByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Fabrics
            .AsNoTracking()
            .Include(f => f.FabricGroup)
            .Where(f => f.Id == id)
            .Select(f => new FabricDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Composition = f.Composition,
                ImageUrl = f.ImageUrl,
                Price = f.Price,
                FabricGroupId = f.FabricGroupId,
                FabricGroupName = f.FabricGroup != null ? f.FabricGroup.Name : null,
                IsAvailable = f.IsAvailable,
                CreatedAt = f.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FabricDTO> AddFabricAsync(CreateFabricDTO dto, CancellationToken cancellationToken = default)
    {
        var fabric = new Fabric
        {
            Name = dto.Name,
            Description = dto.Description,
            Composition = dto.Composition,
            ImageUrl = dto.ImageUrl,
            Price = dto.Price,
            FabricGroupId = dto.FabricGroupId,
            IsAvailable = true,
            CreatedAt = DateTime.Now
        };

        _context.Fabrics.Add(fabric);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetFabricByIdAsync(fabric.Id, cancellationToken) ?? new FabricDTO { Id = fabric.Id, Name = fabric.Name };
    }

    public async Task<FabricDTO?> UpdateFabricAsync(int id, UpdateFabricDTO dto, CancellationToken cancellationToken = default)
    {
        var fabric = await _context.Fabrics.FindAsync(new object[] { id }, cancellationToken);
        if (fabric == null) return null;

        fabric.Name = dto.Name;
        fabric.Description = dto.Description;
        fabric.Composition = dto.Composition;
        fabric.ImageUrl = dto.ImageUrl;
        fabric.Price = dto.Price;
        fabric.FabricGroupId = dto.FabricGroupId;
        fabric.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetFabricByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteFabricAsync(int id, CancellationToken cancellationToken = default)
    {
        var fabric = await _context.Fabrics.FindAsync(new object[] { id }, cancellationToken);
        if (fabric == null) return false;

        _context.Fabrics.Remove(fabric);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FabricGroupDTO> AddFabricGroupAsync(CreateFabricGroupDTO dto, CancellationToken cancellationToken = default)
    {
        var group = new FabricGroup
        {
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.Now
        };

        _context.FabricGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        return new FabricGroupDTO
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            DisplayOrder = group.DisplayOrder,
            CreatedAt = group.CreatedAt
        };
    }

    public async Task<FabricGroupDTO?> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO dto, CancellationToken cancellationToken = default)
    {
        var group = await _context.FabricGroups.FindAsync(new object[] { id }, cancellationToken);
        if (group == null) return null;

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetFabricGroupByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteFabricGroupAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await _context.FabricGroups.FindAsync(new object[] { id }, cancellationToken);
        if (group == null) return false;

        _context.FabricGroups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
