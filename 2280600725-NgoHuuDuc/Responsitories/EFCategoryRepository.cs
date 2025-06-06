using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> GetCategoryByNameAsync(string name, int excludeId = 0)
        {
            // Tìm danh mục theo tên, loại trừ một Id nếu cần (dùng để kiểm tra trùng tên khi cập nhật)
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name && c.Id != excludeId);
        }

        public async Task AddCategoryAsync(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi thêm danh mục, trả về thông báo lỗi chi tiết
                throw new Exception("Lỗi khi thêm danh mục: " + ex.Message);
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            // Cập nhật thông tin danh mục
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Xóa danh mục nếu tìm thấy
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            // Kiểm tra xem danh mục có tồn tại không
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }
    }
}
