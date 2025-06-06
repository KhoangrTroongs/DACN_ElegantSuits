using Microsoft.AspNetCore.Identity;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Claims;

namespace NgoHuuDuc_2280600725.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            // Lấy toàn bộ danh sách user từ repository và chuyển sang DTO
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(MapToUserDTO);
        }

        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            // Lấy user theo Id, nếu có thì chuyển sang DTO
            var user = await _userRepository.GetUserByIdAsync(id);
            return user != null ? MapToUserDTO(user) : null;
        }

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            // Lấy user hiện tại dựa trên context đăng nhập
            var user = await _userRepository.GetCurrentUserAsync();
            return user != null ? MapToUserDTO(user) : null;
        }

        public async Task<UserDTO?> UpdateUserAsync(string id, UpdateUserDTO userDto, IFormFile? avatarFile)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Kiểm tra quyền: chỉ cho phép chính chủ hoặc admin cập nhật thông tin user này
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Administrator") ?? false;
            
            if (currentUserId != id && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this user.");
            }

            // Cập nhật các trường thông tin cá nhân
            user.FullName = userDto.FullName;
            user.DateOfBirth = userDto.DateOfBirth;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Address = userDto.Address;
            user.Gender = userDto.Gender;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                // Nếu có avatar mới, xóa avatar cũ (nếu không phải avatar mặc định)
                if (!string.IsNullOrEmpty(user.AvatarUrl) && !user.AvatarUrl.Contains("default-avatar.png"))
                {
                    var oldAvatarPath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                    if (File.Exists(oldAvatarPath))
                    {
                        File.Delete(oldAvatarPath);
                    }
                }

                // Lưu avatar mới và cập nhật đường dẫn
                user.AvatarUrl = await SaveAvatar(avatarFile);
            }

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                // Nếu cập nhật thất bại, trả về lỗi chi tiết
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return MapToUserDTO(user);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            // Kiểm tra quyền: chỉ cho phép chính chủ hoặc admin xóa user này
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Administrator") ?? false;
            
            if (currentUserId != id && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this user.");
            }

            var result = await _userRepository.DeleteUserAsync(id);
            return result.Succeeded;
        }

        private async Task<string> SaveAvatar(IFormFile avatar)
        {
            // Lưu file avatar vào thư mục wwwroot/images/users và trả về đường dẫn tương đối
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatar.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(fileStream);
            }

            return "/images/users/" + uniqueFileName;
        }

        private UserDTO MapToUserDTO(ApplicationUser user)
        {
            // Chuyển đổi ApplicationUser sang UserDTO để trả về cho client
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                Gender = user.Gender,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
