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
    private readonly ICartApiClient _cartApiClient;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProductApiClient productApiClient,
        ICategoryApiClient categoryApiClient,
        ICartApiClient cartApiClient,
        ILogger<HomeController> logger)
    {
        _productApiClient = productApiClient;
        _categoryApiClient = categoryApiClient;
        _cartApiClient = cartApiClient;
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

        var prodList = await _productApiClient.GetProductsAsync();
        var allProducts = prodList.Select(p => new Product
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

            if (categoryProducts.Any())
            {
                productsByCategory[category.Name] = categoryProducts;
            }
        }

        ViewBag.ProductsByCategory = productsByCategory;
        return View(allProducts.Where(p => !p.IsHidden).Take(12).ToList());
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

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            var res = await _cartApiClient.GetCartAsync(token);
            int count = res.Data?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(new { count });
        }

        int sessionCount = SessionCartService.GetCount(HttpContext.Session);
        return Json(new { count = sessionCount });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? size = null)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                      Request.ContentType?.Contains("application/json") == true;

        if (!string.IsNullOrEmpty(token))
        {
            var req = new AddToCartRequest
            {
                ProductId = productId,
                Quantity = quantity,
                Size = size
            };
            var res = await _cartApiClient.AddToCartAsync(req, token);
            if (isAjax)
            {
                int count = res.Data?.Items.Sum(x => x.Quantity) ?? 1;
                return Json(new { success = res.IsSuccess, count, message = res.IsSuccess ? "Đã thêm vào giỏ hàng thành công!" : (res.Message ?? "Không thể thêm vào giỏ hàng.") });
            }

            if (res.IsSuccess)
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            else
                TempData["ErrorMessage"] = res.Message ?? "Lỗi thêm giỏ hàng";
        }
        else
        {
            var product = await _productApiClient.GetProductByIdAsync(productId);
            if (product != null)
            {
                SessionCartService.AddToCart(HttpContext.Session, product, quantity, size);
                int count = SessionCartService.GetCount(HttpContext.Session);
                if (isAjax)
                {
                    return Json(new { success = true, count, message = "Đã thêm vào giỏ hàng thành công!" });
                }
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            else
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
            }
        }

        return RedirectToAction("Index", "ShoppingCart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int productId, int? id = null)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        int cartCount = 0;

        if (!string.IsNullOrEmpty(token))
        {
            var cartRes = await _cartApiClient.GetCartAsync(token);
            var item = cartRes.Data?.Items.FirstOrDefault(x => x.ProductId == productId || (id.HasValue && x.Id == id.Value));
            if (item != null)
            {
                await _cartApiClient.RemoveCartItemAsync(item.Id, token);
            }
            var fresh = await _cartApiClient.GetCartAsync(token);
            cartCount = fresh.Data?.Items.Sum(x => x.Quantity) ?? 0;
            if (isAjax) return Json(new { success = true, cartCount, message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }
        else
        {
            var cart = SessionCartService.GetSessionCart(HttpContext.Session);
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId || (id.HasValue && x.Id == id.Value));
            if (item != null)
            {
                SessionCartService.RemoveItem(HttpContext.Session, item.Id);
            }
            cartCount = SessionCartService.GetCount(HttpContext.Session);
            if (isAjax) return Json(new { success = true, cartCount, message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        return RedirectToAction("Index", "ShoppingCart");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity, int? id = null)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        int updatedCount = 0;

        if (!string.IsNullOrEmpty(token))
        {
            var cartRes = await _cartApiClient.GetCartAsync(token);
            var item = cartRes.Data?.Items.FirstOrDefault(x => x.ProductId == productId || (id.HasValue && x.Id == id.Value));
            if (item != null)
            {
                await _cartApiClient.UpdateCartItemAsync(item.Id, quantity, token);
                var fresh = await _cartApiClient.GetCartAsync(token);
                updatedCount = fresh.Data?.Items.Sum(x => x.Quantity) ?? 0;
            }
        }
        else
        {
            var cart = SessionCartService.GetSessionCart(HttpContext.Session);
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId || (id.HasValue && x.Id == id.Value));
            if (item != null)
            {
                SessionCartService.UpdateQuantity(HttpContext.Session, item.Id, quantity);
                updatedCount = SessionCartService.GetCount(HttpContext.Session);
            }
        }

        return Json(new { success = true, cartCount = updatedCount });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
