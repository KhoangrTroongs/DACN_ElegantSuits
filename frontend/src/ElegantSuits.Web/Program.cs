using ElegantSuits.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var apiBaseUrl = builder.Configuration["BackendApi:BaseUrl"] ?? "http://localhost:5097";

builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICartApiClient, CartApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IFabricApiClient, FabricApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICouponApiClient, CouponApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));

// Allow untyped HttpClient injection for StatisticsController and PaymentController
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".glb"] = "model/gltf-binary";
contentTypeProvider.Mappings[".gltf"] = "model/gltf+json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
