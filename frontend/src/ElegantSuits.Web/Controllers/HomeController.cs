using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductApiClient _productApiClient;
    private readonly ICategoryApiClient _categoryApiClient;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProductApiClient productApiClient,
        ICategoryApiClient categoryApiClient,
        ILogger<HomeController> logger)
    {
        _productApiClient = productApiClient;
        _categoryApiClient = categoryApiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        if (User.IsInRole("Administrator"))
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var catRes = await _categoryApiClient.GetAllCategoriesAsync();
        var categories = (catRes.Data ?? new List<CategoryViewModel>()).Select(c => new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? ""
        }).ToList();
        ViewBag.Categories = categories;

        var prodRes = await _productApiClient.GetPagedProductsAsync(null, 1, 50);
        var allProducts = (prodRes ?? new PaginatedList<ProductViewModel>()).Select(p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Model3DUrl = p.Model3DUrl,
            Quantity = p.Quantity,
            IsHidden = p.IsHidden,
            CategoryId = p.CategoryId
        }).ToList();

        var productsByCategory = new Dictionary<string, List<Product>>();
        foreach (var category in categories)
        {
            var categoryProducts = allProducts
                .Where(p => p.CategoryId == category.Id && !p.IsHidden)
                .OrderByDescending(p => p.Id)
                .Take(10)
                .ToList();

            productsByCategory[category.Name] = categoryProducts;
        }

        ViewBag.ProductsByCategory = productsByCategory;
        return View(allProducts.Take(12).ToList());
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View(new ContactViewModel());

    [HttpPost]
    public IActionResult Contact(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ với chúng tôi!";
            return RedirectToAction(nameof(Contact));
        }
        return View(model);
    }

    public IActionResult Cart() => RedirectToAction("Index", "ShoppingCart");

    public IActionResult Dashboard() => RedirectToAction("Index", "Statistics");

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
