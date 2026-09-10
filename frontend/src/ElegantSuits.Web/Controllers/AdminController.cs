using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Statistics");
    }

    public IActionResult UpdateLinearCodes()
    {
        return View();
    }
}
