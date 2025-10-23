using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IFabricService
    {
        // FabricGroup methods
        Task<IEnumerable<FabricGroupDTO>> GetAllFabricGroupsAsync();
        Task<FabricGroupDTO?> GetFabricGroupByIdAsync(int id);
        Task<FabricGroupDTO> AddFabricGroupAsync(CreateFabricGroupDTO createFabricGroupDTO);
        Task<FabricGroupDTO> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO updateFabricGroupDTO);
        Task DeleteFabricGroupAsync(int id);

        // Fabric methods
        Task<IEnumerable<FabricDTO>> GetAllFabricsAsync();
        Task<FabricDTO?> GetFabricByIdAsync(int id);
        Task<IEnumerable<FabricDTO>> GetFabricsByGroupAsync(int fabricGroupId);
        Task<IEnumerable<FabricDTO>> GetFabricsByProductIdAsync(int productId);
        Task<FabricDTO> AddFabricAsync(CreateFabricDTO createFabricDTO);
        Task<FabricDTO> UpdateFabricAsync(int id, UpdateFabricDTO updateFabricDTO);
        Task DeleteFabricAsync(int id);

        // FabricProduct methods
        Task<IEnumerable<FabricDTO>> GetFabricsByProductAsync(int productId);
        Task AddFabricToProductAsync(int productId, int fabricId);
        Task RemoveFabricFromProductAsync(int productId, int fabricId);
        Task RemoveAllFabricsFromProductAsync(int productId);
    }
}

