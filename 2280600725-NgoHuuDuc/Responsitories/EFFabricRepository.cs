using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFFabricRepository : IFabricRepository
    {
        private readonly ApplicationDbContext _context;

        public EFFabricRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // FabricGroup methods
        public async Task<IEnumerable<FabricGroup>> GetAllFabricGroupsAsync()
        {
            return await _context.FabricGroups
                .Include(fg => fg.Fabrics)
                .OrderBy(fg => fg.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FabricGroup?> GetFabricGroupByIdAsync(int id)
        {
            return await _context.FabricGroups
                .Include(fg => fg.Fabrics)
                .FirstOrDefaultAsync(fg => fg.Id == id);
        }

        public async Task<FabricGroup> AddFabricGroupAsync(FabricGroup fabricGroup)
        {
            _context.FabricGroups.Add(fabricGroup);
            await _context.SaveChangesAsync();
            return fabricGroup;
        }

        public async Task<FabricGroup> UpdateFabricGroupAsync(FabricGroup fabricGroup)
        {
            _context.FabricGroups.Update(fabricGroup);
            await _context.SaveChangesAsync();
            return fabricGroup;
        }

        public async Task DeleteFabricGroupAsync(int id)
        {
            var fabricGroup = await _context.FabricGroups.FindAsync(id);
            if (fabricGroup != null)
            {
                _context.FabricGroups.Remove(fabricGroup);
                await _context.SaveChangesAsync();
            }
        }

        // Fabric methods
        public async Task<IEnumerable<Fabric>> GetAllFabricsAsync()
        {
            return await _context.Fabrics
                .Include(f => f.FabricGroup)
                .OrderBy(f => f.FabricGroup!.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Fabric?> GetFabricByIdAsync(int id)
        {
            return await _context.Fabrics
                .Include(f => f.FabricGroup)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Fabric>> GetFabricsByGroupAsync(int fabricGroupId)
        {
            return await _context.Fabrics
                .Where(f => f.FabricGroupId == fabricGroupId)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fabric>> GetFabricsByProductIdAsync(int productId)
        {
            return await _context.FabricProducts
                .Where(fp => fp.ProductId == productId && fp.IsAvailable)
                .Include(fp => fp.Fabric)
                .ThenInclude(f => f!.FabricGroup)
                .Select(fp => fp.Fabric!)
                .OrderBy(f => f.FabricGroup!.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Fabric> AddFabricAsync(Fabric fabric)
        {
            _context.Fabrics.Add(fabric);
            await _context.SaveChangesAsync();
            return fabric;
        }

        public async Task<Fabric> UpdateFabricAsync(Fabric fabric)
        {
            _context.Fabrics.Update(fabric);
            await _context.SaveChangesAsync();
            return fabric;
        }

        public async Task DeleteFabricAsync(int id)
        {
            var fabric = await _context.Fabrics.FindAsync(id);
            if (fabric != null)
            {
                _context.Fabrics.Remove(fabric);
                await _context.SaveChangesAsync();
            }
        }

        // FabricProduct methods
        public async Task<IEnumerable<FabricProduct>> GetFabricProductsByProductIdAsync(int productId)
        {
            return await _context.FabricProducts
                .Where(fp => fp.ProductId == productId)
                .Include(fp => fp.Fabric)
                .ThenInclude(f => f!.FabricGroup)
                .ToListAsync();
        }

        public async Task<IEnumerable<FabricProduct>> GetFabricProductsByFabricIdAsync(int fabricId)
        {
            return await _context.FabricProducts
                .Where(fp => fp.FabricId == fabricId)
                .Include(fp => fp.Product)
                .ToListAsync();
        }

        public async Task<FabricProduct?> GetFabricProductByIdAsync(int id)
        {
            return await _context.FabricProducts
                .Include(fp => fp.Fabric)
                .Include(fp => fp.Product)
                .FirstOrDefaultAsync(fp => fp.Id == id);
        }

        public async Task<FabricProduct> AddFabricProductAsync(FabricProduct fabricProduct)
        {
            _context.FabricProducts.Add(fabricProduct);
            await _context.SaveChangesAsync();
            return fabricProduct;
        }

        public async Task DeleteFabricProductAsync(int id)
        {
            var fabricProduct = await _context.FabricProducts.FindAsync(id);
            if (fabricProduct != null)
            {
                _context.FabricProducts.Remove(fabricProduct);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFabricProductsByProductIdAsync(int productId)
        {
            var fabricProducts = await _context.FabricProducts
                .Where(fp => fp.ProductId == productId)
                .ToListAsync();

            if (fabricProducts.Any())
            {
                _context.FabricProducts.RemoveRange(fabricProducts);
                await _context.SaveChangesAsync();
            }
        }
    }
}

