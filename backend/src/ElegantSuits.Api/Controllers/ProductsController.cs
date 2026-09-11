using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Products.Contracts;
using ElegantSuits.Application.Features.Products.Commands.CreateProduct;
using ElegantSuits.Application.Features.Products.Commands.DeleteProduct;
using ElegantSuits.Application.Features.Products.Commands.UpdateProduct;
using ElegantSuits.Application.Features.Products.Queries.GetPagedProducts;
using ElegantSuits.Application.Features.Products.Queries.GetProductById;
using ElegantSuits.Application.Features.Products.Queries.GetProducts;
using ElegantSuits.Application.Features.Products.Queries.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    // GET: api/Products?categoryId=1
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<IReadOnlyList<ProductResponse>>>> GetProducts(
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var products = await _sender.Send(new GetProductsQuery(categoryId), cancellationToken);
        return Ok(ResponseDTO<IReadOnlyList<ProductResponse>>.Success(products));
    }

    // GET: api/Products/paged?categoryId=1&pageIndex=1&pageSize=10
    [HttpGet("paged")]
    public async Task<ActionResult<ResponseDTO<PaginatedList<ProductResponse>>>> GetPagedProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPagedProductsQuery(categoryId, pageIndex, pageSize), cancellationToken);
        return Ok(ResponseDTO<PaginatedList<ProductResponse>>.Success(result));
    }

    // GET: api/Products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseDTO<ProductResponse>>> GetProduct(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product == null)
        {
            return NotFound(ResponseDTO<ProductResponse>.Fail("Product not found."));
        }

        return Ok(ResponseDTO<ProductResponse>.Success(product));
    }

    // GET: api/Products/search?keyword=shirt&pageIndex=1&pageSize=10
    [HttpGet("search")]
    public async Task<ActionResult<ResponseDTO<PaginatedList<ProductResponse>>>> SearchProducts(
        [FromQuery] string keyword,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SearchProductsQuery(keyword, pageIndex, pageSize), cancellationToken);
        return Ok(ResponseDTO<PaginatedList<ProductResponse>>.Success(result));
    }

    // POST: api/Products
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<ProductResponse>>> CreateProduct(
        [FromForm] CreateProductRequest request,
        IFormFile? image,
        IFormFile? model3D,
        CancellationToken cancellationToken)
    {
        Stream? imageStream = image != null ? image.OpenReadStream() : null;
        string? imageFileName = image?.FileName;

        Stream? model3DStream = model3D != null ? model3D.OpenReadStream() : null;
        string? model3DFileName = model3D?.FileName;

        try
        {
            var command = new CreateProductCommand(request, imageStream, imageFileName, model3DStream, model3DFileName);
            var product = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ResponseDTO<ProductResponse>.Success(product));
        }
        finally
        {
            imageStream?.Dispose();
            model3DStream?.Dispose();
        }
    }

    // PUT: api/Products/5
    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<ProductResponse>>> UpdateProduct(
        int id,
        [FromForm] UpdateProductRequest request,
        IFormFile? image,
        IFormFile? model3D,
        CancellationToken cancellationToken)
    {
        Stream? imageStream = image != null ? image.OpenReadStream() : null;
        string? imageFileName = image?.FileName;

        Stream? model3DStream = model3D != null ? model3D.OpenReadStream() : null;
        string? model3DFileName = model3D?.FileName;

        try
        {
            var command = new UpdateProductCommand(id, request, imageStream, imageFileName, model3DStream, model3DFileName);
            var product = await _sender.Send(command, cancellationToken);
            if (product == null)
            {
                return NotFound(ResponseDTO<ProductResponse>.Fail("Product not found."));
            }

            return Ok(ResponseDTO<ProductResponse>.Success(product));
        }
        finally
        {
            imageStream?.Dispose();
            model3DStream?.Dispose();
        }
    }

    // DELETE: api/Products/5
    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteProduct(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProductCommand(id), cancellationToken);
        if (!result)
        {
            return NotFound(ResponseDTO<bool>.Fail("Product not found."));
        }

        return Ok(ResponseDTO<bool>.Success(true, "Product deleted successfully."));
    }
}
