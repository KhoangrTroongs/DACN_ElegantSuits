using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class RoleManagementController : Controller
{
    public IActionResult Index()
    {
        var viewModels = new List<UserRolesViewModel>();
        return View(viewModels);
    }

    public IActionResult EditRoles(string userId)
    {
        var viewModel = new UserRolesViewModel
        {
            UserId = userId,
            UserName = "User",
            Email = "user@example.com",
            CurrentRoles = new List<string> { "Member" },
            AvailableRoles = new List<string> { "Administrator", "Member" },
            SelectedRoles = new List<string> { "Member" }
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult EditRoles(string userId, List<string> roles)
    {
        TempData["Message"] = "Cập nhật quyền thành công";
        return RedirectToAction(nameof(Index));
    }
}
