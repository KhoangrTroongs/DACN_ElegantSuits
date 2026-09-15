using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductApiClient _productApiClient;
    private readonly ICategoryApiClient _categoryApiClient;
    private readonly IFabricApiClient _fabricApiClient;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductApiClient productApiClient,
        ICategoryApiClient categoryApiClient,
        IFabricApiClient fabricApiClient,
        ILogger<ProductController> logger)
    {
        _productApiClient = productApiClient;
        _categoryApiClient = categoryApiClient;
        _fabricApiClient = fabricApiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? categoryId, string? keyword, int pageNumber = 1, string sortBy = "", string order = "asc", string filter = "")
    {
        const int pageSize = 12;

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        var categories = catRes.Data ?? new List<CategoryViewModel>();
        ViewBag.Categories = categories;
        ViewBag.CategoryId = categoryId;
        ViewBag.SelectedCategory = categoryId.HasValue ? categories.FirstOrDefault(c => c.Id == categoryId.Value) : null;
        ViewBag.Keyword = keyword;
        ViewBag.SearchKeyword = keyword;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.SortBy = sortBy;
        ViewBag.Order = order;
        ViewBag.CurrentSort = !string.IsNullOrEmpty(sortBy) ? $"{sortBy}-{order}" : "";
        ViewBag.CurrentFilter = filter;

        var result = await _productApiClient.GetPagedProductsAsync(categoryId, pageNumber, pageSize, keyword);
        var products = result ?? new PaginatedList<ProductViewModel>(new List<ProductViewModel>(), 0, 1, pageSize);

        ViewBag.TotalItems = products.TotalItems;
        ViewBag.TotalPages = products.TotalPages;
        ViewBag.HasPreviousPage = products.HasPreviousPage;
        ViewBag.HasNextPage = products.HasNextPage;

        var mapped = products.Select(p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Model3DUrl = p.Model3DUrl,
            Quantity = p.Quantity,
            IsHidden = p.IsHidden,
            CategoryId = p.CategoryId,
            LinearCode = p.LinearCode,
            Category = p.CategoryName != null ? new Category { Id = p.CategoryId, Name = p.CategoryName } : null
        }).ToList();

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
            {
                mapped = (order?.ToLower() == "desc") 
                    ? mapped.OrderByDescending(p => p.Price).ToList() 
                    : mapped.OrderBy(p => p.Price).ToList();
            }
            else if (sortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                mapped = (order?.ToLower() == "desc") 
                    ? mapped.OrderByDescending(p => p.Name).ToList() 
                    : mapped.OrderBy(p => p.Name).ToList();
            }
        }

        var paginated = new PaginatedList<Product>(mapped, products.TotalItems, products.PageIndex, products.PageSize);
        return View(paginated);
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _productApiClient.GetProductByIdAsync(id);
        if (p == null)
        {
            return NotFound();
        }

        var product = new Product
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Model3DUrl = p.Model3DUrl,
            Quantity = p.Quantity,
            IsHidden = p.IsHidden,
            CategoryId = p.CategoryId,
            LinearCode = p.LinearCode,
            Category = p.CategoryName != null ? new Category { Id = p.CategoryId, Name = p.CategoryName } : null,
            ProductReviews = p.Reviews.Select(r => new ProductReview
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                User = new ApplicationUser { FullName = r.UserName, UserName = r.UserName, Email = r.UserName }
            }).ToList()
        };

        var fabricsRes = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = fabricsRes.Data ?? new List<FabricGroupViewModel>();

        return View(product);
    }

    public IActionResult Search(string? keyword, int pageNumber = 1)
    {
        return RedirectToAction(nameof(Index), new { keyword, pageNumber });
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        var model = new ProductViewModel
        {
            Categories = (catRes.Data ?? new List<CategoryViewModel>())
                .Select(c => new Category { Id = c.Id, Name = c.Name }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            var createDto = new CreateProductViewModel
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                CategoryId = model.CategoryId,
                IsHidden = model.IsHidden,
                LinearCode = model.LinearCode,
                Image = model.Image,
                Model3D = model.Model3D,
                ProfitMargin = model.ProfitMargin
            };
            var res = await _productApiClient.CreateProductAsync(createDto);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", res.Message ?? "Lỗi tạo sản phẩm.");
        }

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        model.Categories = (catRes.Data ?? new List<CategoryViewModel>())
            .Select(c => new Category { Id = c.Id, Name = c.Name }).ToList();
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _productApiClient.GetProductByIdAsync(id);
        if (p == null) return NotFound();

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        var editModel = new ProductViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Quantity = p.Quantity,
            CategoryId = p.CategoryId,
            IsHidden = p.IsHidden,
            ExistingModel3DUrl = p.Model3DUrl,
            ExistingImageUrl = p.ImageUrl,
            LinearCode = p.LinearCode,
            Categories = (catRes.Data ?? new List<CategoryViewModel>())
                .Select(c => new Category { Id = c.Id, Name = c.Name }).ToList()
        };

        return View(editModel);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var updateDto = new UpdateProductViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                CategoryId = model.CategoryId,
                IsHidden = model.IsHidden,
                Model3DUrl = model.ExistingModel3DUrl,
                ImageUrl = model.ExistingImageUrl,
                LinearCode = model.LinearCode,
                Image = model.Image,
                Model3D = model.Model3D,
                ProfitMargin = model.ProfitMargin
            };
            var res = await _productApiClient.UpdateProductAsync(id, updateDto);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", res.Message ?? "Lỗi cập nhật sản phẩm.");
        }

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        model.Categories = (catRes.Data ?? new List<CategoryViewModel>())
            .Select(c => new Category { Id = c.Id, Name = c.Name }).ToList();
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _productApiClient.GetProductByIdAsync(id);
        if (p == null) return NotFound();

        var product = new Product
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Quantity = p.Quantity,
            CategoryId = p.CategoryId,
            Category = p.CategoryName != null ? new Category { Name = p.CategoryName } : null
        };
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _productApiClient.DeleteProductAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ManageProductSizes(int id)
    {
        var p = await _productApiClient.GetProductByIdAsync(id);
        if (p == null) return NotFound();

        var vm = new ManageProductSizesViewModel
        {
            Product = new Product { Id = p.Id, Name = p.Name, Price = p.Price },
            ProductSizes = new List<ProductSize>(),
            AvailableSizes = new List<string> { "S", "M", "L", "XL", "XXL" },
            Sizes = new List<ProductSizeViewModel>()
        };
        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int productId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        var token = HttpContext.Session.GetString("JwtToken");
        var res = await _productApiClient.AddReviewAsync(productId, rating, comment, token);
        if (res.IsSuccess)
        {
            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
        }
        else
        {
            TempData["Error"] = res.Message ?? "Không thể gửi đánh giá.";
        }

        return RedirectToAction(nameof(Details), new { id = productId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int productId, int reviewId)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var res = await _productApiClient.DeleteReviewAsync(productId, reviewId, token);
        if (res.IsSuccess)
        {
            TempData["Success"] = "Đã xóa đánh giá thành công.";
        }
        else
        {
            TempData["Error"] = res.Message ?? "Không thể xóa đánh giá.";
        }

        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
