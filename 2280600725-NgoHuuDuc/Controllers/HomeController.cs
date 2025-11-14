using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ICouponService _couponService;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, ICouponService couponService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _couponService = couponService;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            if (User.IsInRole("Administrator"))
            {
                // Nếu là admin thì chuyển sang trang Dashboard thay vì trang chủ
                return RedirectToAction(nameof(Dashboard));
            }

            // Lấy danh sách danh mục sản phẩm
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            // Dictionary để lưu trữ sản phẩm theo từng danh mục
            var productsByCategory = new Dictionary<string, List<Product>>();

            // Lấy 10 sản phẩm mới nhất của mỗi danh mục
            foreach (var category in categories)
            {
                var categoryProducts = await _context.Products
                    .Where(p => p.CategoryId == category.Id && !p.IsHidden)
                    .OrderByDescending(p => p.Id) // Sắp xếp theo ID giảm dần (mới nhất trước)
                    .Take(10)
                    .ToListAsync();

                productsByCategory.Add(category.Name, categoryProducts);
            }

            // Gán dữ liệu sản phẩm theo danh mục cho ViewBag để truyền sang view
            ViewBag.ProductsByCategory = productsByCategory;

            // Lấy danh sách coupon đang active và chưa hết hạn
            var activeCoupons = await _couponService.GetAllCouponsAsync();
            var validCoupons = activeCoupons
                .Where(c => c.IsActive && !c.IsExpired && !c.IsDepleted)
                .OrderByDescending(c => c.DiscountPercentage)
                .Take(6) // Lấy tối đa 6 coupon
                .ToList();
            ViewBag.ActiveCoupons = validCoupons;

            return View();
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Đếm số lượng sản phẩm
                ViewBag.ProductCount = await _context.Products.CountAsync();

                // Đếm số lượng người dùng
                ViewBag.UserCount = await _userManager.Users.CountAsync();

                // Đếm số lượng danh mục
                ViewBag.CategoryCount = await _context.Categories.CountAsync();

                // Đếm số lượng đơn hàng
                ViewBag.OrderCount = await _context.Orders.CountAsync();

                // Lấy 5 sản phẩm mới nhất
                ViewBag.RecentProducts = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .Take(5)
                    .ToListAsync();

                // Lấy 5 người dùng mới nhất và lấy vai trò của từng người dùng
                var recentUsers = await _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();

                var recentUsersViewModel = new List<object>();
                foreach (var user in recentUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    recentUsersViewModel.Add(new
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "User"
                    });
                }

                ViewBag.RecentUsers = recentUsersViewModel;

                return View();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception khi load dashboard
                _logger.LogError(ex, "Error in Dashboard action");
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Contact(int? productId)
        {
            if (productId.HasValue)
            {
                var product = await _context.Products.FindAsync(productId.Value);
                if (product != null)
                {
                    ViewBag.ProductName = product.Name;
                    ViewBag.ProductId = product.Id;
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Ở đây bạn có thể thêm code để gửi email hoặc lưu thông tin liên hệ vào database
                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ với chúng tôi. Chúng tôi sẽ phản hồi trong thời gian sớm nhất!";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            // Lấy giỏ hàng của user hiện tại (theo User.Identity.Name)
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart?.Items ?? new List<CartItem>());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string size = null)
        {
            // Thêm sản phẩm vào giỏ hàng, có thể kèm theo size
            var userId = User.Identity.Name;
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Kiểm tra số lượng sản phẩm còn lại
            if (product.Quantity <= 0)
            {
                return Json(new {
                    success = false,
                    message = "Sản phẩm đã hết hàng"
                });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            // Nếu có size thì tìm cartItem theo productId và size, nếu không thì chỉ theo productId
            var cartItem = size != null
                ? cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size)
                : cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == null);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl,
                    Size = size
                };
                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            // Không giảm số lượng sản phẩm khi thêm vào giỏ hàng
            // Số lượng sản phẩm sẽ giảm khi đơn hàng được tạo

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new {
                success = true,
                cartCount = cart.Items.Sum(x => x.Quantity)
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            // Xóa sản phẩm khỏi giỏ hàng
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    // Không cần tăng lại số lượng sản phẩm khi xóa khỏi giỏ hàng
                    // vì số lượng sản phẩm không bị giảm khi thêm vào giỏ hàng

                    _context.CartItems.Remove(item);
                    cart.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return Json(new {
                        success = true,
                        cartCount = cart.Items.Sum(x => x.Quantity)
                    });
                }
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                // Kiểm tra authentication
                if (!User.Identity.IsAuthenticated)
                {
                    Console.WriteLine("GetCartCount - User not authenticated");
                    return Json(new { count = 0 });
                }

                var userId = User.Identity.Name;
                Console.WriteLine($"GetCartCount - User ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("GetCartCount - User ID is null or empty");
                    return Json(new { count = 0 });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var count = cart?.Items.Sum(x => x.Quantity) ?? 0;
                Console.WriteLine($"GetCartCount - Cart count: {count}");

                return Json(new { count });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy số lượng sản phẩm trong giỏ hàng
                Console.WriteLine($"GetCartCount Error: {ex.Message}");
                return Json(new { count = 0 });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1) return Json(new { success = false });

            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    // Lấy sản phẩm để kiểm tra số lượng tồn kho
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                    // Tính toán sự thay đổi số lượng
                    int quantityDifference = quantity - item.Quantity;

                    // Kiểm tra xem có đủ số lượng sản phẩm không
                    if (quantityDifference > 0 && product.Quantity < quantityDifference)
                    {
                        return Json(new {
                            success = false,
                            message = $"Chỉ còn {product.Quantity} sản phẩm trong kho"
                        });
                    }

                    // Không cập nhật số lượng sản phẩm trong kho
                    // Số lượng sản phẩm sẽ giảm khi đơn hàng được tạo

                    // Cập nhật số lượng trong giỏ hàng
                    item.Quantity = quantity;
                    cart.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return Json(new {
                        success = true,
                        cartCount = cart.Items.Sum(x => x.Quantity)
                    });
                }
            }

            return Json(new { success = false });
        }

        // Test method để debug GetCartCount
        [HttpGet]
        public async Task<IActionResult> TestCartCount()
        {
            try
            {
                Console.WriteLine("=== TEST CART COUNT START ===");
                Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");

                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new {
                        success = false,
                        message = "User not authenticated",
                        isAuthenticated = false
                    });
                }

                var userId = User.Identity.Name;
                Console.WriteLine($"UserId: {userId}");

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                Console.WriteLine($"Cart found: {cart != null}");
                Console.WriteLine($"Cart items count: {cart?.Items?.Count ?? 0}");

                var totalQuantity = cart?.Items.Sum(x => x.Quantity) ?? 0;
                Console.WriteLine($"Total quantity: {totalQuantity}");

                return Json(new {
                    success = true,
                    isAuthenticated = true,
                    userId = userId,
                    cartFound = cart != null,
                    itemsCount = cart?.Items?.Count ?? 0,
                    totalQuantity = totalQuantity,
                    message = "Test completed successfully"
                });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi test lấy số lượng sản phẩm trong giỏ hàng
                Console.WriteLine($"TestCartCount Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
