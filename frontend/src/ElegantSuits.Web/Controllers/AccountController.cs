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
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthApiClient authApiClient,
        ICartApiClient cartApiClient,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _cartApiClient = cartApiClient;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string? GetToken()
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? User.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrEmpty(token) && string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            HttpContext.Session.SetString("JwtToken", token);
        }
        return token;
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
    public async Task<IActionResult> Details()
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/profile");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<UserDTO>>();
            if (apiRes?.Data != null)
            {
                var u = apiRes.Data;
                var vm = new UserDetailsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    AvatarUrl = !string.IsNullOrEmpty(u.AvatarUrl) ? u.AvatarUrl : "/images/users/default-avatar.png",
                    IsActive = u.IsActive,
                    DateOfBirth = u.DateOfBirth,
                    CreatedAt = u.CreatedAt
                };
                return View(vm);
            }
        }

        var fallback = new UserDetailsViewModel
        {
            UserName = User.Identity?.Name ?? "",
            Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            FullName = User.Identity?.Name ?? ""
        };
        return View(fallback);
    }

    [Authorize]
    public IActionResult Profile() => RedirectToAction(nameof(Details));

    [Authorize]
    public async Task<IActionResult> Update()
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/profile");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<UserDTO>>();
            if (apiRes?.Data != null)
            {
                var u = apiRes.Data;
                var vm = new UserDetailsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    AvatarUrl = u.AvatarUrl,
                    DateOfBirth = u.DateOfBirth
                };
                return View(vm);
            }
        }

        var fallback = new UserDetailsViewModel
        {
            Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            FullName = User.Identity?.Name ?? ""
        };
        return View(fallback);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUser(string id)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/{id}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<UserDTO>>();
            if (apiRes?.Data != null)
            {
                var u = apiRes.Data;
                var vm = new UserDetailsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    AvatarUrl = u.AvatarUrl,
                    DateOfBirth = u.DateOfBirth
                };
                return View("Update", vm);
            }
        }

        TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Update(UserDetailsViewModel model, IFormFile? AvatarFile)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";

        string? avatarUrl = model.AvatarUrl;
        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
            Directory.CreateDirectory(webRoot);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(AvatarFile.FileName)}";
            var filePath = Path.Combine(webRoot, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await AvatarFile.CopyToAsync(stream);
            }
            avatarUrl = $"/images/users/{fileName}";
        }

        var updateDto = new
        {
            FullName = model.FullName,
            DateOfBirth = model.DateOfBirth,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            AvatarUrl = avatarUrl
        };

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdminUpdatingOther = User.IsInRole("Administrator") && !string.IsNullOrEmpty(model.Id) && model.Id != currentUserId;

        var targetUrl = isAdminUpdatingOther 
            ? $"{baseUrl}/api/Users/{model.Id}" 
            : $"{baseUrl}/api/Users/profile";

        var request = new HttpRequestMessage(HttpMethod.Put, targetUrl)
        {
            Content = JsonContent.Create(updateDto)
        };
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            if (isAdminUpdatingOther)
            {
                return RedirectToAction(nameof(UserDetails), new { id = model.Id });
            }
            return RedirectToAction(nameof(Details));
        }

        TempData["ErrorMessage"] = "Cập nhật thông tin thất bại.";
        return View("Update", model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        var list = new List<UserDetailsViewModel>();
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<List<UserDTO>>>();
            if (apiRes?.Data != null)
            {
                list = apiRes.Data.Select(u => new UserDetailsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    AvatarUrl = !string.IsNullOrEmpty(u.AvatarUrl) ? u.AvatarUrl : "/images/users/default-avatar.png",
                    IsActive = u.IsActive,
                    DateOfBirth = u.DateOfBirth,
                    CreatedAt = u.CreatedAt
                }).ToList();
            }
        }
        return View(list);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAllRoles()
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/roles");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<List<string>>>();
            if (apiRes?.Data != null) return Json(apiRes.Data);
        }

        return Json(new List<string> { "Administrator", "User" });
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/{userId}/roles");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<List<string>>>();
            if (apiRes?.Data != null) return Json(apiRes.Data);
        }

        return Json(new List<string> { "User" });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateRoles([FromBody] Dictionary<string, List<string>> changedRoles)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/api/Users/roles")
        {
            Content = JsonContent.Create(changedRoles)
        };
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Cập nhật vai trò thành công." });
        }

        return Json(new { success = false, message = "Không thể cập nhật vai trò." });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> LockUser(string id)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/Users/{id}/lock");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Khóa người dùng thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể khóa người dùng.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/Users/{id}/unlock");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Mở khóa người dùng thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể mở khóa người dùng.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UserDetails(string id)
    {
        var token = GetToken();
        var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/Users/{id}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var res = await _httpClient.SendAsync(request);
        if (res.IsSuccessStatusCode)
        {
            var apiRes = await res.Content.ReadFromJsonAsync<ResponseDTO<UserDTO>>();
            if (apiRes?.Data != null)
            {
                var u = apiRes.Data;
                var vm = new UserDetailsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    AvatarUrl = !string.IsNullOrEmpty(u.AvatarUrl) ? u.AvatarUrl : "/images/users/default-avatar.png",
                    IsActive = u.IsActive,
                    DateOfBirth = u.DateOfBirth,
                    CreatedAt = u.CreatedAt
                };
                return View("Details", vm);
            }
        }
        return NotFound();
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
