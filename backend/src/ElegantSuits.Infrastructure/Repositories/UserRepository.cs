using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Users.Contracts;
using ElegantSuits.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        return users.Select(MapToDTO);
    }

    public async Task<UserDTO?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user != null ? MapToDTO(user) : null;
    }

    public async Task<UserDTO?> UpdateUserAsync(string id, UpdateUserDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        user.FullName = dto.FullName;
        user.DateOfBirth = dto.DateOfBirth;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.Gender = dto.Gender;

        await _userManager.UpdateAsync(user);
        return MapToDTO(user);
    }

    private static UserDTO MapToDTO(ApplicationUser u)
    {
        return new UserDTO
        {
            Id = u.Id,
            Email = u.Email ?? "",
            FullName = u.FullName,
            DateOfBirth = u.DateOfBirth,
            PhoneNumber = u.PhoneNumber,
            Address = u.Address,
            AvatarUrl = u.AvatarUrl ?? "",
            Gender = u.Gender,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }
}
