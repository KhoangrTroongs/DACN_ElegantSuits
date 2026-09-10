using ElegantSuits.Application.Features.Fabrics.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IFabricRepository
{
    Task<IEnumerable<FabricGroupDTO>> GetAllFabricGroupsAsync(CancellationToken cancellationToken = default);
    Task<FabricGroupDTO?> GetFabricGroupByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FabricDTO>> GetAllFabricsAsync(CancellationToken cancellationToken = default);
    Task<FabricDTO?> GetFabricByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FabricDTO> AddFabricAsync(CreateFabricDTO dto, CancellationToken cancellationToken = default);
    Task<FabricDTO?> UpdateFabricAsync(int id, UpdateFabricDTO dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteFabricAsync(int id, CancellationToken cancellationToken = default);
    Task<FabricGroupDTO> AddFabricGroupAsync(CreateFabricGroupDTO dto, CancellationToken cancellationToken = default);
    Task<FabricGroupDTO?> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteFabricGroupAsync(int id, CancellationToken cancellationToken = default);
}
