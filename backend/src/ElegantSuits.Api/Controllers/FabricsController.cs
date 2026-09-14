using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Fabrics.Commands.CreateFabric;
using ElegantSuits.Application.Features.Fabrics.Commands.CreateFabricGroup;
using ElegantSuits.Application.Features.Fabrics.Commands.DeleteFabric;
using ElegantSuits.Application.Features.Fabrics.Commands.DeleteFabricGroup;
using ElegantSuits.Application.Features.Fabrics.Commands.UpdateFabric;
using ElegantSuits.Application.Features.Fabrics.Commands.UpdateFabricGroup;
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

    // PUT: api/Fabrics/{id}
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<FabricDTO>>> UpdateFabric(
        int id,
        [FromBody] UpdateFabricDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateFabricCommand(id, dto), cancellationToken);
        if (result == null)
            return NotFound(ResponseDTO<FabricDTO>.Fail("Không tìm thấy vải"));
        return Ok(ResponseDTO<FabricDTO>.Success(result));
    }

    // DELETE: api/Fabrics/{id}
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteFabric(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteFabricCommand(id), cancellationToken);
        if (!result)
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy vải"));
        return Ok(ResponseDTO<bool>.Success(true));
    }

    // POST: api/Fabrics/groups
    [HttpPost("groups")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<FabricGroupDTO>>> CreateFabricGroup(
        [FromBody] CreateFabricGroupDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateFabricGroupCommand(dto), cancellationToken);
        return Ok(ResponseDTO<FabricGroupDTO>.Success(result));
    }

    // PUT: api/Fabrics/groups/{id}
    [HttpPut("groups/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<FabricGroupDTO>>> UpdateFabricGroup(
        int id,
        [FromBody] UpdateFabricGroupDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateFabricGroupCommand(id, dto), cancellationToken);
        if (result == null)
            return NotFound(ResponseDTO<FabricGroupDTO>.Fail("Không tìm thấy nhóm vải"));
        return Ok(ResponseDTO<FabricGroupDTO>.Success(result));
    }

    // DELETE: api/Fabrics/groups/{id}
    [HttpDelete("groups/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteFabricGroup(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteFabricGroupCommand(id), cancellationToken);
        if (!result)
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy nhóm vải"));
        return Ok(ResponseDTO<bool>.Success(true));
    }
}
