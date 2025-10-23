using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public interface IFabricRepository
    {
        // FabricGroup methods
        Task<IEnumerable<FabricGroup>> GetAllFabricGroupsAsync();
        Task<FabricGroup?> GetFabricGroupByIdAsync(int id);
        Task<FabricGroup> AddFabricGroupAsync(FabricGroup fabricGroup);
        Task<FabricGroup> UpdateFabricGroupAsync(FabricGroup fabricGroup);
        Task DeleteFabricGroupAsync(int id);

        // Fabric methods
        Task<IEnumerable<Fabric>> GetAllFabricsAsync();
        Task<Fabric?> GetFabricByIdAsync(int id);
        Task<IEnumerable<Fabric>> GetFabricsByGroupAsync(int fabricGroupId);
        Task<IEnumerable<Fabric>> GetFabricsByProductIdAsync(int productId);
        Task<Fabric> AddFabricAsync(Fabric fabric);
        Task<Fabric> UpdateFabricAsync(Fabric fabric);
        Task DeleteFabricAsync(int id);

        // FabricProduct methods
        Task<IEnumerable<FabricProduct>> GetFabricProductsByProductIdAsync(int productId);
        Task<IEnumerable<FabricProduct>> GetFabricProductsByFabricIdAsync(int fabricId);
        Task<FabricProduct?> GetFabricProductByIdAsync(int id);
        Task<FabricProduct> AddFabricProductAsync(FabricProduct fabricProduct);
        Task DeleteFabricProductAsync(int id);
        Task DeleteFabricProductsByProductIdAsync(int productId);
    }
}

