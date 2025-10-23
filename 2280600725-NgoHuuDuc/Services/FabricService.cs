using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class FabricService : IFabricService
    {
        private readonly IFabricRepository _fabricRepository;

        public FabricService(IFabricRepository fabricRepository)
        {
            _fabricRepository = fabricRepository;
        }

        // FabricGroup methods
        public async Task<IEnumerable<FabricGroupDTO>> GetAllFabricGroupsAsync()
        {
            var fabricGroups = await _fabricRepository.GetAllFabricGroupsAsync();
            return fabricGroups.Select(fg => MapToFabricGroupDTO(fg));
        }

        public async Task<FabricGroupDTO?> GetFabricGroupByIdAsync(int id)
        {
            var fabricGroup = await _fabricRepository.GetFabricGroupByIdAsync(id);
            return fabricGroup != null ? MapToFabricGroupDTO(fabricGroup) : null;
        }

        public async Task<FabricGroupDTO> AddFabricGroupAsync(CreateFabricGroupDTO createFabricGroupDTO)
        {
            var fabricGroup = new FabricGroup
            {
                Name = createFabricGroupDTO.Name,
                Description = createFabricGroupDTO.Description,
                DisplayOrder = createFabricGroupDTO.DisplayOrder,
                CreatedAt = DateTime.Now
            };

            var result = await _fabricRepository.AddFabricGroupAsync(fabricGroup);
            return MapToFabricGroupDTO(result);
        }

        public async Task<FabricGroupDTO> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO updateFabricGroupDTO)
        {
            var fabricGroup = await _fabricRepository.GetFabricGroupByIdAsync(id);
            if (fabricGroup == null)
                throw new KeyNotFoundException($"FabricGroup with id {id} not found");

            fabricGroup.Name = updateFabricGroupDTO.Name;
            fabricGroup.Description = updateFabricGroupDTO.Description;
            fabricGroup.DisplayOrder = updateFabricGroupDTO.DisplayOrder;

            var result = await _fabricRepository.UpdateFabricGroupAsync(fabricGroup);
            return MapToFabricGroupDTO(result);
        }

        public async Task DeleteFabricGroupAsync(int id)
        {
            await _fabricRepository.DeleteFabricGroupAsync(id);
        }

        // Fabric methods
        public async Task<IEnumerable<FabricDTO>> GetAllFabricsAsync()
        {
            var fabrics = await _fabricRepository.GetAllFabricsAsync();
            return fabrics.Select(f => MapToFabricDTO(f));
        }

        public async Task<FabricDTO?> GetFabricByIdAsync(int id)
        {
            var fabric = await _fabricRepository.GetFabricByIdAsync(id);
            return fabric != null ? MapToFabricDTO(fabric) : null;
        }

        public async Task<IEnumerable<FabricDTO>> GetFabricsByGroupAsync(int fabricGroupId)
        {
            var fabrics = await _fabricRepository.GetFabricsByGroupAsync(fabricGroupId);
            return fabrics.Select(f => MapToFabricDTO(f));
        }

        public async Task<IEnumerable<FabricDTO>> GetFabricsByProductIdAsync(int productId)
        {
            var fabrics = await _fabricRepository.GetFabricsByProductIdAsync(productId);
            return fabrics.Select(f => MapToFabricDTO(f));
        }

        public async Task<FabricDTO> AddFabricAsync(CreateFabricDTO createFabricDTO)
        {
            var fabric = new Fabric
            {
                Name = createFabricDTO.Name,
                Description = createFabricDTO.Description,
                Composition = createFabricDTO.Composition,
                ImageUrl = createFabricDTO.ImageUrl,
                Price = createFabricDTO.Price,
                FabricGroupId = createFabricDTO.FabricGroupId,
                IsAvailable = true,
                CreatedAt = DateTime.Now
            };

            var result = await _fabricRepository.AddFabricAsync(fabric);
            return MapToFabricDTO(result);
        }

        public async Task<FabricDTO> UpdateFabricAsync(int id, UpdateFabricDTO updateFabricDTO)
        {
            var fabric = await _fabricRepository.GetFabricByIdAsync(id);
            if (fabric == null)
                throw new KeyNotFoundException($"Fabric with id {id} not found");

            fabric.Name = updateFabricDTO.Name;
            fabric.Description = updateFabricDTO.Description;
            fabric.Composition = updateFabricDTO.Composition;
            fabric.ImageUrl = updateFabricDTO.ImageUrl;
            fabric.Price = updateFabricDTO.Price;
            fabric.FabricGroupId = updateFabricDTO.FabricGroupId;
            fabric.IsAvailable = updateFabricDTO.IsAvailable;

            var result = await _fabricRepository.UpdateFabricAsync(fabric);
            return MapToFabricDTO(result);
        }

        public async Task DeleteFabricAsync(int id)
        {
            await _fabricRepository.DeleteFabricAsync(id);
        }

        // FabricProduct methods
        public async Task<IEnumerable<FabricDTO>> GetFabricsByProductAsync(int productId)
        {
            var fabrics = await _fabricRepository.GetFabricsByProductIdAsync(productId);
            return fabrics.Select(f => MapToFabricDTO(f));
        }

        public async Task AddFabricToProductAsync(int productId, int fabricId)
        {
            // Validate that the fabric exists before creating the association
            var fabric = await _fabricRepository.GetFabricByIdAsync(fabricId);
            if (fabric == null)
            {
                throw new KeyNotFoundException($"Fabric with id {fabricId} not found");
            }

            var fabricProduct = new FabricProduct
            {
                ProductId = productId,
                FabricId = fabricId,
                IsAvailable = true,
                CreatedAt = DateTime.Now
            };

            await _fabricRepository.AddFabricProductAsync(fabricProduct);
        }

        public async Task RemoveFabricFromProductAsync(int productId, int fabricId)
        {
            var fabricProducts = await _fabricRepository.GetFabricProductsByProductIdAsync(productId);
            var fabricProduct = fabricProducts.FirstOrDefault(fp => fp.FabricId == fabricId);

            if (fabricProduct != null)
            {
                await _fabricRepository.DeleteFabricProductAsync(fabricProduct.Id);
            }
        }

        public async Task RemoveAllFabricsFromProductAsync(int productId)
        {
            await _fabricRepository.DeleteFabricProductsByProductIdAsync(productId);
        }

        // Helper methods
        private FabricDTO MapToFabricDTO(Fabric fabric)
        {
            return new FabricDTO
            {
                Id = fabric.Id,
                Name = fabric.Name,
                Description = fabric.Description,
                Composition = fabric.Composition,
                ImageUrl = fabric.ImageUrl,
                Price = fabric.Price,
                FabricGroupId = fabric.FabricGroupId,
                FabricGroupName = fabric.FabricGroup?.Name,
                IsAvailable = fabric.IsAvailable,
                CreatedAt = fabric.CreatedAt
            };
        }

        private FabricGroupDTO MapToFabricGroupDTO(FabricGroup fabricGroup)
        {
            return new FabricGroupDTO
            {
                Id = fabricGroup.Id,
                Name = fabricGroup.Name,
                Description = fabricGroup.Description,
                DisplayOrder = fabricGroup.DisplayOrder,
                CreatedAt = fabricGroup.CreatedAt,
                Fabrics = fabricGroup.Fabrics?.Select(f => MapToFabricDTO(f))
            };
        }
    }
}

