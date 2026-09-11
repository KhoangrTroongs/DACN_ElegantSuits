using System.Security.Claims;
using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Users.Contracts;
using ElegantSuits.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: api/Users
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<IEnumerable<UserDTO>>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        return Ok(ResponseDTO<IEnumerable<UserDTO>>.Success(users));
    }

    // GET: api/Users/profile
    [HttpGet("profile")]
    public async Task<ActionResult<ResponseDTO<UserDTO>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<UserDTO>.Fail("User not authenticated."));
        }

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
        }

        return Ok(ResponseDTO<UserDTO>.Success(user));
    }

    // PUT: api/Users/profile
    [HttpPut("profile")]
    public async Task<ActionResult<ResponseDTO<UserDTO>>> UpdateProfile(
        [FromBody] UpdateUserDTO dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<UserDTO>.Fail("User not authenticated."));
        }

        var updated = await _userRepository.UpdateUserAsync(userId, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
        }

        return Ok(ResponseDTO<UserDTO>.Success(updated));
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<UserDTO>>> GetUserById(string id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound(ResponseDTO<UserDTO>.Fail("Không tìm thấy người dùng."));
        }

        return Ok(ResponseDTO<UserDTO>.Success(user));
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<UserDTO>>> UpdateUser(
        string id,
        [FromBody] UpdateUserDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _userRepository.UpdateUserAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(ResponseDTO<UserDTO>.Fail("Không tìm thấy người dùng."));
        }

        return Ok(ResponseDTO<UserDTO>.Success(updated, "Cập nhật thông tin người dùng thành công."));
    }

    // GET: api/Users/roles
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<List<string>>>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        return Ok(ResponseDTO<List<string>>.Success(roles));
    }

    // GET: api/Users/{id}/roles
    [HttpGet("{id}/roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<List<string>>>> GetUserRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ResponseDTO<List<string>>.Fail("Không tìm thấy người dùng."));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(ResponseDTO<List<string>>.Success(roles));
    }

    // PUT: api/Users/roles
    [HttpPut("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> UpdateRoles([FromBody] Dictionary<string, List<string>> changedRoles)
    {
        foreach (var kvp in changedRoles)
        {
            var user = await _userManager.FindByIdAsync(kvp.Key);
            if (user == null) continue;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, kvp.Value);
        }

        return Ok(ResponseDTO<bool>.Success(true, "Cập nhật vai trò thành công."));
    }

    // POST: api/Users/{id}/lock
    [HttpPost("{id}/lock")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> LockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy người dùng."));
        }

        user.IsActive = false;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        await _userManager.UpdateAsync(user);
        return Ok(ResponseDTO<bool>.Success(true, "Khóa người dùng thành công."));
    }

    // POST: api/Users/{id}/unlock
    [HttpPost("{id}/unlock")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy người dùng."));
        }

        user.IsActive = true;
        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);
        return Ok(ResponseDTO<bool>.Success(true, "Mở khóa người dùng thành công."));
    }
}
