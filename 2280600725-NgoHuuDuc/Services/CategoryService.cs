using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ILogger<CategoryService> logger)
        {
            // Hàm khởi tạo nhận repository và logger thông qua dependency injection
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories.Select(MapToCategoryDTO);
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            return category != null ? MapToCategoryDTO(category) : null;
        }

        public async Task<CategoryDTO> AddCategoryAsync(CreateCategoryDTO categoryDto)
        {
            // Kiểm tra xem đã có danh mục nào trùng tên chưa
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Danh mục với tên '{categoryDto.Name}' đã tồn tại.");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            await _categoryRepository.AddCategoryAsync(category);
            
            // Lấy lại danh mục vừa tạo từ database để trả về đầy đủ thông tin (bao gồm cả Id)
            var createdCategory = await _categoryRepository.GetCategoryByIdAsync(category.Id);
            return MapToCategoryDTO(createdCategory);
        }

        public async Task<CategoryDTO?> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDto)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            // Kiểm tra xem có danh mục khác trùng tên không (loại trừ chính nó)
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(categoryDto.Name, id);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Danh mục khác với tên '{categoryDto.Name}' đã tồn tại.");
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            await _categoryRepository.UpdateCategoryAsync(category);
            
            // Lấy lại danh mục sau khi cập nhật để trả về thông tin mới nhất
            var updatedCategory = await _categoryRepository.GetCategoryByIdAsync(id);
            return MapToCategoryDTO(updatedCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Xóa danh mục theo Id
            await _categoryRepository.DeleteCategoryAsync(id);
            return true;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            // Kiểm tra xem danh mục có tồn tại không
            return await _categoryRepository.CategoryExistsAsync(id);
        }

        private CategoryDTO MapToCategoryDTO(Category category)
        {
            // Chuyển đổi đối tượng Category sang CategoryDTO để trả về cho client
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
