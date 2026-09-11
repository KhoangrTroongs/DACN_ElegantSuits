using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Auth.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Auth.Commands.ExternalLogin;

public record ExternalLoginCommand(ExternalLoginDTO DTO) : IRequest<AuthResponseDTO>;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, AuthResponseDTO>
{
    private readonly IAuthService _authService;

    public ExternalLoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDTO> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        return await _authService.ExternalLoginAsync(command.DTO);
    }
}
