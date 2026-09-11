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
    private readonly ICartApiClient _cartApiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthApiClient authApiClient,
        ICartApiClient cartApiClient,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _cartApiClient = cartApiClient;
        _configuration = configuration;
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

            // Sync guest session cart to user account in database
            await SessionCartService.SyncSessionCartToApiAsync(HttpContext.Session, _cartApiClient, data.Token);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.UserId ?? ""),
                new Claim(ClaimTypes.Name, data.UserName ?? model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("JwtToken", data.Token)
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

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var googleClientId = _configuration["Authentication:Google:ClientId"];
        var googleClientSecret = _configuration["Authentication:Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret) || googleClientId == "YOUR_GOOGLE_CLIENT_ID")
        {
            TempData["ErrorMessage"] = "Chức năng đăng nhập Google chưa được cấu hình ClientId/ClientSecret trong appsettings.json. Vui lòng cấu hình để sử dụng.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Lỗi từ dịch vụ bên ngoài: {remoteError}";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var authResult = await HttpContext.AuthenticateAsync("ExternalCookie");
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            TempData["ErrorMessage"] = "Không thể xác thực thông tin tài khoản từ Google.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var claims = authResult.Principal.Claims.ToList();
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var providerKey = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        // Clean up the temporary external cookie
        await HttpContext.SignOutAsync("ExternalCookie");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
        {
            TempData["ErrorMessage"] = "Không thể lấy thông tin email từ Google.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var res = await _authApiClient.ExternalLoginAsync(new ExternalLoginRequest
        {
            Provider = "Google",
            ProviderKey = providerKey,
            Email = email,
            FullName = name
        });

        if (!res.IsSuccess || res.Data == null || string.IsNullOrEmpty(res.Data.Token))
        {
            TempData["ErrorMessage"] = res.Message ?? "Đăng nhập Google thất bại.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var data = res.Data;
        HttpContext.Session.SetString("JwtToken", data.Token);

        var identityClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, data.UserId ?? providerKey),
            new Claim(ClaimTypes.Name, data.UserName ?? name ?? email),
            new Claim(ClaimTypes.Email, email),
            new Claim("JwtToken", data.Token)
        };

        if (data.Roles != null)
        {
            foreach (var role in data.Roles)
            {
                identityClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(identityClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = data.Expiration ?? DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        // Sync session cart to API
        await SessionCartService.SyncSessionCartToApiAsync(HttpContext.Session, _cartApiClient, data.Token);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
