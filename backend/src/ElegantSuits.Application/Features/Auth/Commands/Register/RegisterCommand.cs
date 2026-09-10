using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Auth.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterUserDTO DTO) : IRequest<AuthResponseDTO>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDTO>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDTO> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(command.DTO);
    }
}
