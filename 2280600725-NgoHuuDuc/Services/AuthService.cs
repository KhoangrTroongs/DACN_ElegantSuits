using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // Đoạn này là phần khai báo các biến readonly để lưu các service/phụ thuộc cần thiết cho AuthService:
        // - UserManager<ApplicationUser>: Quản lý các thao tác liên quan đến người dùng (tạo, tìm kiếm, cập nhật, xóa user)
        // - SignInManager<ApplicationUser>: Quản lý đăng nhập/đăng xuất người dùng
        // - RoleManager<IdentityRole>: Quản lý các vai trò (role) của người dùng
        // - IConfiguration: Đọc cấu hình ứng dụng (ví dụ: lấy secret key cho JWT)
        // - ILogger<AuthService>: Ghi log cho service này

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            // Hàm khởi tạo (constructor) nhận các service/phụ thuộc thông qua dependency injection và gán vào các biến readonly ở trên
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                // Nếu không tìm thấy user với email này thì trả về lỗi
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email hoặc mật khẩu không đúng."
                };
            }

            if (!user.IsActive)
            {
                // Nếu tài khoản bị vô hiệu hóa thì không cho đăng nhập
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn đã bị vô hiệu hóa."
                };
            }

            // Kiểm tra đăng nhập bằng mật khẩu, không bật lockoutOnFailure
            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, false);
            if (!result.Succeeded)
            {
                // Nếu đăng nhập thất bại thì trả về lỗi
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email hoặc mật khẩu không đúng."
                };
            }

            // Lấy danh sách vai trò của người dùng và sinh JWT token
            var userRoles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtTokenAsync(user.Id, user.UserName, userRoles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Đăng nhập thành công.",
                Token = token,
                Expiration = DateTime.Now.AddDays(7),
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles.ToList()
            };
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                // Nếu email đã tồn tại thì trả về lỗi
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email đã được sử dụng."
                };
            }

            // Tạo mới một ApplicationUser từ thông tin đăng ký
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                DateOfBirth = registerDto.DateOfBirth,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                Gender = registerDto.Gender,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            // Thực hiện tạo user với password
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                // Nếu có lỗi khi tạo user thì trả về danh sách lỗi
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Đăng ký không thành công.",
                    Roles = errors
                };
            }

            // Thêm user vào role "Customer" mặc định nếu chưa có
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }
            await _userManager.AddToRoleAsync(user, "Customer");

            // Đăng nhập user sau khi đăng ký thành công
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Lấy vai trò và sinh JWT token cho user mới
            var userRoles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtTokenAsync(user.Id, user.UserName, userRoles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Đăng ký thành công.",
                Token = token,
                Expiration = DateTime.Now.AddDays(7),
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles.ToList()
            };
        }

        public async Task<bool> LogoutAsync()
        {
            // Đăng xuất người dùng hiện tại
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<string> GenerateJwtTokenAsync(string userId, string userName, IList<string> roles)
        {
            // Tạo danh sách claims cho JWT token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm các claim về vai trò vào token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Lấy key bí mật từ cấu hình để ký token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "DefaultSecretKeyWithAtLeast32Characters!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(7);

            // Tạo JWT token với các thông tin cấu hình
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"] ?? "https://localhost:5001",
                audience: _configuration["JWT:ValidAudience"] ?? "https://localhost:5001",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Trả về chuỗi token đã mã hóa
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
