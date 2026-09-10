using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class DatabaseController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
