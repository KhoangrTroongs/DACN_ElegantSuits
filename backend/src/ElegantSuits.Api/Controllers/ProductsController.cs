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

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ElegantSuits.Infrastructure.Persistence;
using ElegantSuits.Domain.Entities;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _context;

    public ProductsController(ISender sender, ApplicationDbContext context)
    {
        _sender = sender;
        _context = context;
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

    // GET: api/Products/paged?categoryId=1&keyword=shirt&pageIndex=1&pageSize=10
    [HttpGet("paged")]
    public async Task<ActionResult<ResponseDTO<PaginatedList<ProductResponse>>>> GetPagedProducts(
        [FromQuery] int? categoryId,
        [FromQuery] string? keyword,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPagedProductsQuery(categoryId, pageIndex, pageSize, keyword), cancellationToken);
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

    // POST: api/Products/5/reviews
    [HttpPost("{id:int}/reviews")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ResponseDTO<ProductReviewResponse>>> AddReview(
        int id,
        [FromBody] CreateProductReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<ProductReviewResponse>.Fail("Vui lòng đăng nhập để đánh giá."));
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(ResponseDTO<ProductReviewResponse>.Fail("Đánh giá phải từ 1 đến 5 sao."));
        }

        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null)
        {
            return NotFound(ResponseDTO<ProductReviewResponse>.Fail("Không tìm thấy sản phẩm."));
        }

        var existingReview = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId, cancellationToken);

        if (existingReview != null)
        {
            existingReview.Rating = request.Rating;
            existingReview.Comment = request.Comment;
            existingReview.CreatedAt = DateTime.Now;
        }
        else
        {
            existingReview = new ProductReview
            {
                ProductId = id,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now
            };
            _context.ProductReviews.Add(existingReview);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        var response = new ProductReviewResponse
        {
            Id = existingReview.Id,
            ProductId = id,
            UserId = userId,
            UserName = user?.FullName ?? user?.UserName ?? user?.Email ?? "Khách hàng",
            Rating = existingReview.Rating,
            Comment = existingReview.Comment,
            CreatedAt = existingReview.CreatedAt
        };

        return Ok(ResponseDTO<ProductReviewResponse>.Success(response, "Cảm ơn bạn đã đánh giá sản phẩm!"));
    }

    // DELETE: api/Products/5/reviews/10
    [HttpDelete("{productId:int}/reviews/{reviewId:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteReview(
        int productId,
        int reviewId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdmin = User.IsInRole("Administrator");

        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.ProductId == productId, cancellationToken);

        if (review == null)
        {
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy đánh giá."));
        }

        if (review.UserId != userId && !isAdmin)
        {
            return Forbid();
        }

        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ResponseDTO<bool>.Success(true, "Đã xóa đánh giá thành công."));
    }
}
