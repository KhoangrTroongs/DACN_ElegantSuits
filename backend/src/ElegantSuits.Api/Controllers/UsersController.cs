using System.Security.Claims;
using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Users.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
}
