using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Auth.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginUserDTO DTO) : IRequest<AuthResponseDTO>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDTO>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDTO> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(command.DTO);
    }
}
