using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Text.RegularExpressions;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _logger = logger;
            _environment = environment;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<IEnumerable<ProductDTO>>>> GetProducts([FromQuery] int? categoryId)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.GetProductsByCategoryAsync(categoryId)
                    : await _productService.GetProductsByCategoryAsync(categoryId, false);
                return Ok(ResponseDTO<IEnumerable<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy danh sách sản phẩm
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, ResponseDTO<IEnumerable<ProductDTO>>.Fail("An error occurred while retrieving products."));
            }
        }

        // GET: api/Products/paged?categoryId=1&pageIndex=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult<ResponseDTO<PaginatedList<ProductDTO>>>> GetPagedProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize)
                    : await _productService.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize, false);
                return Ok(ResponseDTO<PaginatedList<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged products");
                return StatusCode(500, ResponseDTO<PaginatedList<ProductDTO>>.Fail("An error occurred while retrieving products."));
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }

                // Nếu sản phẩm bị ẩn và người dùng không phải admin, trả về NotFound
                if (product.IsHidden && !User.IsInRole("Administrator"))
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }

                return Ok(ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy sản phẩm theo id
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while retrieving the product."));
            }
        }

        // GET: api/Products/search?keyword=shirt&pageIndex=1&pageSize=10
        [HttpGet("search")]
        public async Task<ActionResult<ResponseDTO<PaginatedList<ProductDTO>>>> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.SearchProductsAsync(keyword, pageIndex, pageSize)
                    : await _productService.SearchProductsAsync(keyword, pageIndex, pageSize, false);
                return Ok(ResponseDTO<PaginatedList<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with keyword {Keyword}", keyword);
                return StatusCode(500, ResponseDTO<PaginatedList<ProductDTO>>.Fail("An error occurred while searching for products."));
            }
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> CreateProduct([FromForm] CreateProductDTO productDto, IFormFile? image)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào hợp lệ
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<ProductDTO>.Fail("Invalid product data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // Thêm sản phẩm mới
                var product = await _productService.AddProductAsync(productDto, image);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi thêm sản phẩm
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while creating the product."));
            }
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> UpdateProduct(int id, [FromForm] UpdateProductDTO productDto, IFormFile? image)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào hợp lệ
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<ProductDTO>.Fail("Invalid product data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // Cập nhật sản phẩm
                var product = await _productService.UpdateProductAsync(id, productDto, image);
                if (product == null)
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }

                return Ok(ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi cập nhật sản phẩm
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while updating the product."));
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteProduct(int id)
        {
            try
            {
                // Xóa sản phẩm theo id
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Product not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Product deleted successfully."));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi xóa sản phẩm
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while deleting the product."));
            }
        }

        // POST: api/Products/{id}/upload-image
        [HttpPost("{id}/upload-image")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<string>>> UploadProductImage(int id, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(ResponseDTO<string>.Fail("No image file provided."));
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(ResponseDTO<string>.Fail("Invalid file format. Allowed: jpg, jpeg, png, gif, webp"));
                }

                // Lấy thông tin sản phẩm
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ResponseDTO<string>.Fail("Product not found."));
                }

                // Tạo thư mục images/products nếu chưa tồn tại
                var productImagesPath = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(productImagesPath))
                {
                    Directory.CreateDirectory(productImagesPath);
                }

                // Tạo tên file: tên sản phẩm_xx.extension
                // Làm sạch tên sản phẩm (loại bỏ ký tự đặc biệt)
                var cleanProductName = Regex.Replace(product.Name, @"[^a-zA-Z0-9\u00C0-\u024F\u1E00-\u1EFF\s]", "")
                    .Replace(" ", "_")
                    .Trim();

                // Đếm số ảnh hiện có của sản phẩm
                var existingImages = Directory.GetFiles(productImagesPath, $"{cleanProductName}_*.*");
                var imageNumber = existingImages.Length + 1;
                var fileName = $"{cleanProductName}_{imageNumber:D2}{extension}";
                var filePath = Path.Combine(productImagesPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Đường dẫn tương đối để lưu vào database
                var relativeUrl = $"/images/products/{fileName}";

                // Cập nhật ImageUrl của sản phẩm
                var updateDto = new UpdateProductDTO
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    IsHidden = product.IsHidden,
                    ImageUrl = relativeUrl
                };
                await _productService.UpdateProductAsync(id, updateDto, null);

                _logger.LogInformation("Image uploaded for product {ProductId}: {FileName}", id, fileName);

                return Ok(ResponseDTO<string>.Success(relativeUrl, "Image uploaded successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for product {Id}", id);
                return StatusCode(500, ResponseDTO<string>.Fail("An error occurred while uploading the image."));
            }
        }

        // POST: api/Products/upload-temp-image
        // Upload ảnh tạm cho sản phẩm mới (chưa có ID)
        [HttpPost("upload-temp-image")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<string>>> UploadTempImage(IFormFile image, [FromForm] string productName)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(ResponseDTO<string>.Fail("No image file provided."));
                }

                if (string.IsNullOrWhiteSpace(productName))
                {
                    return BadRequest(ResponseDTO<string>.Fail("Product name is required."));
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(ResponseDTO<string>.Fail("Invalid file format. Allowed: jpg, jpeg, png, gif, webp"));
                }

                // Tạo thư mục images/products nếu chưa tồn tại
                var productImagesPath = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(productImagesPath))
                {
                    Directory.CreateDirectory(productImagesPath);
                }

                // Làm sạch tên sản phẩm
                var cleanProductName = Regex.Replace(productName, @"[^a-zA-Z0-9\u00C0-\u024F\u1E00-\u1EFF\s]", "")
                    .Replace(" ", "_")
                    .Trim();

                // Đếm số ảnh hiện có của sản phẩm
                var existingImages = Directory.GetFiles(productImagesPath, $"{cleanProductName}_*.*");
                var imageNumber = existingImages.Length + 1;
                var fileName = $"{cleanProductName}_{imageNumber:D2}{extension}";
                var filePath = Path.Combine(productImagesPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var relativeUrl = $"/images/products/{fileName}";

                _logger.LogInformation("Temp image uploaded: {FileName}", fileName);

                return Ok(ResponseDTO<string>.Success(relativeUrl, "Image uploaded successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading temp image");
                return StatusCode(500, ResponseDTO<string>.Fail("An error occurred while uploading the image."));
            }
        }
    }
}
