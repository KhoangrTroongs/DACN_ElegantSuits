﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize] // Allow all authenticated users
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order - Show user's own orders (or all for admin)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            IQueryable<Order> ordersQuery = _context.Orders.Include(o => o.User);

            // If not admin, only show user's own orders
            if (!User.IsInRole("Administrator"))
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == user.Id);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check if user owns this order or is admin
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!User.IsInRole("Administrator") && order.UserId != user.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        // POST: Order/UpdateStatus - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // User actions
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check if user owns this order
            var user = await _userManager.GetUserAsync(User);
            if (user == null || order.UserId != user.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        public async Task<IActionResult> OrderFailed(int? id)
        {
            if (id.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order != null)
                {
                    // Check if user owns this order
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null && order.UserId == user.Id)
                    {
                        return View(order);
                    }
                }
            }

            return View();
        }
    }
}
