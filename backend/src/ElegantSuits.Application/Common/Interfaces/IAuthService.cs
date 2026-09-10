using ElegantSuits.Application.Features.Auth.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto);
    Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto);
}
