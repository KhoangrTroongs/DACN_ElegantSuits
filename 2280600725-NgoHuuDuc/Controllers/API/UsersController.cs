using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Claims;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<IEnumerable<UserDTO>>>> GetUsers()
        {
            try
            {
                // Lấy danh sách tất cả user (chỉ cho admin)
                var users = await _userService.GetAllUsersAsync();
                return Ok(ResponseDTO<IEnumerable<UserDTO>>.Success(users));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy danh sách user
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, ResponseDTO<IEnumerable<UserDTO>>.Fail("An error occurred while retrieving users."));
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> GetUser(string id)
        {
            try
            {
                // Kiểm tra quyền: chỉ admin hoặc chính chủ mới được xem thông tin user này
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }
                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy user theo id
                _logger.LogError(ex, "Error getting user {Id}", id);
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while retrieving the user."));
            }
        }

        // GET: api/Users/current
        [HttpGet("current")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> GetCurrentUser()
        {
            try
            {
                // Lấy user hiện tại dựa trên context đăng nhập
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }
                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy user hiện tại
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while retrieving the current user."));
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> UpdateUser(string id, [FromForm] UpdateUserDTO userDto, IFormFile? avatarFile)
        {
            try
            {
                // Kiểm tra quyền: chỉ admin hoặc chính chủ mới được cập nhật user này
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }

                // Kiểm tra dữ liệu đầu vào hợp lệ
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<UserDTO>.Fail("Invalid user data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // Cập nhật thông tin user
                var user = await _userService.UpdateUserAsync(id, userDto, avatarFile);
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }

                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (UnauthorizedAccessException)
            {
                // Nếu không có quyền thì trả về 403
                return Forbid();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi cập nhật user
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while updating the user."));
            }
        }


    }
}
