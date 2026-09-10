using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Fabrics.Commands.CreateFabric;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using ElegantSuits.Application.Features.Fabrics.Queries.GetFabricGroups;
using ElegantSuits.Application.Features.Fabrics.Queries.GetFabrics;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FabricsController : ControllerBase
{
    private readonly ISender _sender;

    public FabricsController(ISender sender)
    {
        _sender = sender;
    }

    // GET: api/Fabrics
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<IEnumerable<FabricDTO>>>> GetAllFabrics(CancellationToken cancellationToken)
    {
        var fabrics = await _sender.Send(new GetFabricsQuery(), cancellationToken);
        return Ok(ResponseDTO<IEnumerable<FabricDTO>>.Success(fabrics));
    }

    // GET: api/Fabrics/groups
    [HttpGet("groups")]
    public async Task<ActionResult<ResponseDTO<IEnumerable<FabricGroupDTO>>>> GetAllFabricGroups(CancellationToken cancellationToken)
    {
        var groups = await _sender.Send(new GetFabricGroupsQuery(), cancellationToken);
        return Ok(ResponseDTO<IEnumerable<FabricGroupDTO>>.Success(groups));
    }

    // POST: api/Fabrics
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<FabricDTO>>> CreateFabric(
        [FromBody] CreateFabricDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateFabricCommand(dto), cancellationToken);
        return Ok(ResponseDTO<FabricDTO>.Success(result));
    }
}
