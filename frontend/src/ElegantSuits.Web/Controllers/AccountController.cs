using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthApiClient authApiClient, ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var res = await _authApiClient.LoginAsync(model);
        if (res.IsSuccess && res.Data != null && !string.IsNullOrEmpty(res.Data.Token))
        {
            var data = res.Data;
            // Store token in session for API clients
            HttpContext.Session.SetString("JwtToken", data.Token);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.UserId ?? ""),
                new Claim(ClaimTypes.Name, data.UserName ?? model.Email),
                new Claim(ClaimTypes.Email, model.Email)
            };

            if (data.Roles != null)
            {
                foreach (var role in data.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = data.Expiration ?? DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", res.Message ?? "Đăng nhập thất bại. Kiểm tra email hoặc mật khẩu.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var res = await _authApiClient.RegisterAsync(model);
        if (res.IsSuccess)
        {
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError("", res.Message ?? "Đăng ký thất bại.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove("JwtToken");
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult Details()
    {
        var vm = new UserDetailsViewModel
        {
            UserName = User.Identity?.Name ?? "",
            Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            FullName = User.Identity?.Name ?? ""
        };
        return View(vm);
    }

    [Authorize]
    public IActionResult Profile() => RedirectToAction(nameof(Details));

    [Authorize]
    public IActionResult Update()
    {
        var vm = new UserDetailsViewModel
        {
            UserName = User.Identity?.Name ?? "",
            Email = User.FindFirstValue(ClaimTypes.Email) ?? ""
        };
        return View(vm);
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Index()
    {
        var list = new List<UserDetailsViewModel>();
        return View(list);
    }

    public IActionResult Lockout()
    {
        return View();
    }
}
