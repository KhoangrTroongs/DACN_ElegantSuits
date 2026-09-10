using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class CustomDesignController : Controller
{
    private readonly IFabricApiClient _fabricApiClient;
    private readonly IProductApiClient _productApiClient;
    private readonly ILogger<CustomDesignController> _logger;

    public CustomDesignController(
        IFabricApiClient fabricApiClient,
        IProductApiClient productApiClient,
        ILogger<CustomDesignController> logger)
    {
        _fabricApiClient = fabricApiClient;
        _productApiClient = productApiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var res = await _fabricApiClient.GetFabricGroupsAsync();
            var groups = (res.Data ?? new List<FabricGroupViewModel>()).Select(g => new FabricGroupDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description ?? "",
                DisplayOrder = g.DisplayOrder,
                Fabrics = g.Fabrics.Select(f => new FabricDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description ?? "",
                    Composition = f.Composition,
                    Price = f.PricePerMeter,
                    ImageUrl = f.ImageUrl,
                    FabricGroupId = g.Id,
                    FabricGroupName = g.Name,
                    IsAvailable = f.IsAvailable
                }).ToList()
            }).ToList();

            return View(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải danh sách nhóm vải");
            return View(new List<FabricGroupDTO>());
        }
    }

    public async Task<IActionResult> FabricGroup(int id)
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        var group = res.Data?.FirstOrDefault(g => g.Id == id);
        if (group == null)
        {
            return NotFound();
        }

        var dto = new FabricGroupDTO
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description ?? "",
            DisplayOrder = group.DisplayOrder,
            Fabrics = group.Fabrics.Select(f => new FabricDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description ?? "",
                Composition = f.Composition,
                Price = f.PricePerMeter,
                ImageUrl = f.ImageUrl,
                FabricGroupId = group.Id,
                FabricGroupName = group.Name,
                IsAvailable = f.IsAvailable
            }).ToList()
        };

        return View(dto);
    }

    public async Task<IActionResult> FabricDetail(int id)
    {
        var res = await _fabricApiClient.GetFabricByIdAsync(id);
        if (!res.IsSuccess || res.Data == null)
        {
            return NotFound();
        }

        var f = res.Data;
        var dto = new FabricDTO
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description ?? "",
            Composition = f.Composition,
            Price = f.PricePerMeter,
            ImageUrl = f.ImageUrl,
            FabricGroupId = f.FabricGroupId,
            FabricGroupName = f.FabricGroupName,
            IsAvailable = f.IsAvailable
        };

        return View(dto);
    }

    public async Task<IActionResult> SelectProduct()
    {
        var res = await _productApiClient.GetPagedProductsAsync(null, 1, 50);
        var products = (res ?? new PaginatedList<ProductViewModel>()).Select(p => new ProductDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            IsHidden = p.IsHidden,
            CategoryId = p.CategoryId,
            Quantity = p.Quantity
        }).ToList();

        return View(products);
    }

    public async Task<IActionResult> DesignProduct(int productId)
    {
        var p = await _productApiClient.GetProductByIdAsync(productId);
        if (p == null)
        {
            TempData["ErrorMessage"] = "Sản phẩm không tìm thấy";
            return RedirectToAction(nameof(SelectProduct));
        }

        var product = new ProductDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            IsHidden = p.IsHidden,
            CategoryId = p.CategoryId,
            Quantity = p.Quantity
        };

        var fabricRes = await _fabricApiClient.GetFabricGroupsAsync();
        var fabricGroups = (fabricRes.Data ?? new List<FabricGroupViewModel>()).Select(g => new FabricGroupDTO
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description ?? "",
            DisplayOrder = g.DisplayOrder,
            Fabrics = g.Fabrics.Select(f => new FabricDTO
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description ?? "",
                Composition = f.Composition,
                Price = f.PricePerMeter,
                ImageUrl = f.ImageUrl,
                FabricGroupId = g.Id,
                FabricGroupName = g.Name,
                IsAvailable = f.IsAvailable
            }).ToList()
        }).ToList();

        var vm = new DesignProductViewModel
        {
            Product = product,
            FabricGroups = fabricGroups
        };

        return View(vm);
    }
}
