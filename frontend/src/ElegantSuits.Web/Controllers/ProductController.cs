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

    public async Task<IActionResult> Index(int? categoryId, int pageNumber = 1, string sortBy = "", string order = "asc", string filter = "")
    {
        const int pageSize = 12;

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        var categories = catRes.Data ?? new List<CategoryViewModel>();
        ViewBag.Categories = categories;
        ViewBag.CategoryId = categoryId;
        ViewBag.SelectedCategory = categoryId.HasValue ? categories.FirstOrDefault(c => c.Id == categoryId.Value) : null;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.SortBy = sortBy;
        ViewBag.Order = order;
        ViewBag.CurrentSort = !string.IsNullOrEmpty(sortBy) ? $"{sortBy}-{order}" : "";
        ViewBag.CurrentFilter = filter;

        var result = await _productApiClient.GetPagedProductsAsync(categoryId, pageNumber, pageSize);
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
            Category = p.CategoryName != null ? new Category { Id = p.CategoryId, Name = p.CategoryName } : null
        };

        var fabricsRes = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = fabricsRes.Data ?? new List<FabricGroupViewModel>();

        return View(product);
    }

    public async Task<IActionResult> Search(string keyword, int pageNumber = 1)
    {
        const int pageSize = 12;
        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        ViewBag.Categories = catRes.Data ?? new List<CategoryViewModel>();
        ViewBag.Keyword = keyword;
        ViewBag.SearchKeyword = keyword;

        var result = await _productApiClient.SearchProductsAsync(keyword, pageNumber, pageSize);
        var products = result ?? new PaginatedList<ProductViewModel>(new List<ProductViewModel>(), 0, 1, pageSize);

        ViewBag.ResultCount = products.TotalItems;
        ViewBag.TotalItems = products.TotalItems;
        ViewBag.TotalPages = products.TotalPages;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.HasPreviousPage = products.HasPreviousPage;
        ViewBag.HasNextPage = products.HasNextPage;

        var catDict = (catRes.Data ?? new List<CategoryViewModel>())
            .ToDictionary(c => c.Id, c => c.Name);

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
            Category = new Category
            {
                Id = p.CategoryId,
                Name = !string.IsNullOrWhiteSpace(p.CategoryName)
                    ? p.CategoryName
                    : (catDict.TryGetValue(p.CategoryId, out var cName) ? cName : "Sản phẩm")
            }
        }).ToList();

        var paginated = new PaginatedList<Product>(mapped, products.TotalItems, products.PageIndex, products.PageSize);
        return View("Search", paginated);
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
}
