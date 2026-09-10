using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Auth.Commands.Login;
using ElegantSuits.Application.Features.Auth.Commands.Register;
using ElegantSuits.Application.Features.Auth.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDTO<AuthResponseDTO>>> Login(
        [FromBody] LoginUserDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(dto), cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(ResponseDTO<AuthResponseDTO>.Fail(result.Message ?? "Login failed."));
        }

        return Ok(ResponseDTO<AuthResponseDTO>.Success(result));
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDTO<AuthResponseDTO>>> Register(
        [FromBody] RegisterUserDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterCommand(dto), cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ResponseDTO<AuthResponseDTO>.Fail(result.Message ?? "Registration failed."));
        }

        return Ok(ResponseDTO<AuthResponseDTO>.Success(result));
    }
}
