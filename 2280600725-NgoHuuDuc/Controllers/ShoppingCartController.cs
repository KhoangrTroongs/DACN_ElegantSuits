using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Extensions;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICouponService _couponService;

        public ShoppingCartController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICouponService couponService)
        {
            _context = context;
            _userManager = userManager;
            _couponService = couponService;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy giỏ hàng của user, bao gồm các sản phẩm trong giỏ
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return View(new List<CartItem>());
                }

                return View(cart.Items);
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi load giỏ hàng
                Console.WriteLine($"Error in ShoppingCart Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your cart.";
                return View(new List<CartItem>());
            }
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = User.Identity?.Name;
                Console.WriteLine($"Checkout attempted by user: {userId}"); // Debug logging

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy giỏ hàng của user
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return RedirectToAction("Index", "Home");
                }

                // Truyền CartItems vào ViewBag để load available coupons
                ViewBag.CartItems = cart.Items.Select(i => new { productId = i.ProductId }).ToList();
                ViewBag.HasFreeship = false; // Có thể cập nhật logic này nếu có freeship

                var order = new Order();
                return View(order);
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi vào trang checkout
                Console.WriteLine($"Error in Checkout: {ex.Message}"); // Debug logging
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Log các lỗi validate để debug
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine($"Validation error: {error.ErrorMessage}");
                        }
                    }
                    return View(order);
                }

                // Lấy thông tin người dùng
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items to your cart before checkout.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin user thực tế từ UserManager
                var user = await _userManager.FindByNameAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không thể tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                // Calculate total price before discount
                var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                var discountAmount = 0m;
                var couponCode = order.CouponCode;

                // Validate and apply coupon if provided
                if (!string.IsNullOrWhiteSpace(couponCode))
                {
                    var validationResult = await _couponService.ValidateCouponAsync(couponCode, totalPrice);
                    if (!validationResult.IsValid)
                    {
                        TempData["ErrorMessage"] = validationResult.ErrorMessage;
                        return View(order);
                    }

                    // Calculate discount amount
                    discountAmount = (totalPrice * validationResult.Coupon.DiscountPercentage) / 100;
                }

                // Tạo đơn hàng mới
                var newOrder = new Order
                {
                    UserId = user.Id, // Sử dụng ID thực của người dùng
                    OrderDate = DateTime.Now,
                    TotalPrice = totalPrice - discountAmount,
                    Status = OrderStatus.Pending,
                    ShippingAddress = order.ShippingAddress,
                    Notes = order.Notes,
                    CouponCode = !string.IsNullOrWhiteSpace(couponCode) ? couponCode.ToUpper() : null,
                    DiscountAmount = discountAmount
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // Tạo chi tiết đơn hàng cho từng sản phẩm trong giỏ
                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Kiểm tra số lượng tồn kho
                        if (product.Quantity < item.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' chỉ còn {product.Quantity} sản phẩm trong kho.";
                            _context.Orders.Remove(newOrder);
                            await _context.SaveChangesAsync();
                            return View(order);
                        }

                        // Tạo chi tiết đơn hàng
                        var orderDetail = new OrderDetail
                        {
                            OrderId = newOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Size = item.Size // Thêm thông tin size
                        };

                        _context.Add(orderDetail);

                        // Cập nhật số lượng sản phẩm tồn kho
                        product.Quantity -= item.Quantity;
                        _context.Update(product);
                    }
                }

                // Xóa giỏ hàng sau khi đặt hàng thành công
                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                // Decrement coupon quantity if coupon was used
                if (!string.IsNullOrWhiteSpace(couponCode))
                {
                    await _couponService.DecrementCouponQuantityAsync(couponCode);
                }

                return RedirectToAction("OrderCompleted", new { id = newOrder.Id });
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi đặt hàng
                Console.WriteLine($"Error in Checkout: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult OrderCompleted(int id)
        {
            // Trả về view xác nhận đơn hàng đã hoàn thành
            return View(id);
        }

        public async Task<IActionResult> GetOrderSummary()
        {
            // Kiểm tra nếu đây là một yêu cầu AJAX để trả về partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return PartialView("_OrderSummaryPartial", new List<CartItem>());
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    return PartialView("_OrderSummaryPartial", new List<CartItem>());
                }

                return PartialView("_OrderSummaryPartial", cart.Items);
            }
            else
            {
                // Nếu không phải AJAX thì chuyển hướng về trang MyOrders
                return RedirectToAction(nameof(MyOrders));
            }
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            try
            {
                // Lấy user hiện tại
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy danh sách đơn hàng của user, bao gồm chi tiết đơn hàng và sản phẩm
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                if (orders == null || orders.Count == 0)
                {
                    ViewBag.Message = "You have no orders yet.";
                    return View(new List<Order>());
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi lấy danh sách đơn hàng
                Console.WriteLine($"Error in MyOrders: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your orders.";
                return View(new List<Order>());
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                // Lấy user hiện tại
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy chi tiết đơn hàng của user
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found or you don't have permission to view it.";
                    return RedirectToAction(nameof(MyOrders));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi lấy chi tiết đơn hàng
                Console.WriteLine($"Error in OrderDetails: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your order details.";
                return RedirectToAction(nameof(MyOrders));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidateCoupon(string couponCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
                }

                var validationResult = await _couponService.ValidateCouponAsync(couponCode);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                return Json(new
                {
                    success = true,
                    discountPercentage = validationResult.Coupon.DiscountPercentage,
                    message = $"Áp dụng mã giảm giá thành công! Giảm {validationResult.Coupon.DiscountPercentage}%"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidateCoupon: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xác thực mã giảm giá" });
            }
        }
    }
}
