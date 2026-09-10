using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Categories.Commands.CreateCategory;
using ElegantSuits.Application.Features.Categories.Commands.DeleteCategory;
using ElegantSuits.Application.Features.Categories.Commands.UpdateCategory;
using ElegantSuits.Application.Features.Categories.Contracts;
using ElegantSuits.Application.Features.Categories.Queries.GetCategories;
using ElegantSuits.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    // GET: api/Categories
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<IEnumerable<CategoryDTO>>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(ResponseDTO<IEnumerable<CategoryDTO>>.Success(result));
    }

    // GET: api/Categories/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseDTO<CategoryDTO>>> GetCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(ResponseDTO<CategoryDTO>.Fail("Category not found."));
        }

        return Ok(ResponseDTO<CategoryDTO>.Success(result));
    }

    // POST: api/Categories
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<CategoryDTO>>> CreateCategory(
        [FromBody] CreateCategoryDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateCategoryCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, ResponseDTO<CategoryDTO>.Success(result));
    }

    // PUT: api/Categories/5
    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<CategoryDTO>>> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateCategoryCommand(id, dto), cancellationToken);
        if (result == null)
        {
            return NotFound(ResponseDTO<CategoryDTO>.Fail("Category not found."));
        }

        return Ok(ResponseDTO<CategoryDTO>.Success(result));
    }

    // DELETE: api/Categories/5
    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        if (!result)
        {
            return NotFound(ResponseDTO<bool>.Fail("Category not found."));
        }

        return Ok(ResponseDTO<bool>.Success(true, "Category deleted successfully."));
    }
}
