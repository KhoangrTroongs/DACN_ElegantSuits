using ElegantSuits.Application.Features.Users.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserDTO>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDTO?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<UserDTO?> UpdateUserAsync(string id, UpdateUserDTO userDto, CancellationToken cancellationToken = default);
}
