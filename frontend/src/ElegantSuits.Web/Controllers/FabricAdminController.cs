using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class FabricAdminController : Controller
{
    private readonly IFabricApiClient _fabricApiClient;
    private readonly ILogger<FabricAdminController> _logger;

    public FabricAdminController(
        IFabricApiClient fabricApiClient,
        ILogger<FabricAdminController> logger)
    {
        _fabricApiClient = fabricApiClient;
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

    private List<FabricGroupDTO> MapToGroupDTOs(List<FabricGroupViewModel>? groups)
    {
        return (groups ?? new List<FabricGroupViewModel>()).Select(g => new FabricGroupDTO
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description ?? "",
            DisplayOrder = g.DisplayOrder,
            Fabrics = g.Fabrics?.Select(f => new FabricDTO
            {
                Id = f.Id,
                Name = f.Name,
                Composition = f.Composition ?? f.Material ?? "",
                Price = f.Price > 0 ? f.Price : f.PricePerMeter,
                ImageUrl = f.ImageUrl ?? "",
                FabricGroupId = g.Id,
                FabricGroupName = g.Name,
                IsAvailable = f.IsAvailable
            }) ?? new List<FabricDTO>()
        }).ToList();
    }

    public IActionResult Index() => RedirectToAction(nameof(FabricGroups));

    public async Task<IActionResult> FabricGroups()
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        var groups = MapToGroupDTOs(res.Data);
        return View(groups);
    }

    public IActionResult CreateFabricGroup() => View(new CreateFabricGroupDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFabricGroup(CreateFabricGroupDTO dto)
    {
        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _fabricApiClient.CreateFabricGroupAsync(dto, token);
            if (res.IsSuccess)
            {
                TempData["Success"] = "Thêm nhóm vải thành công";
                return RedirectToAction(nameof(FabricGroups));
            }
            TempData["Error"] = res.Message ?? "Không thể thêm nhóm vải.";
        }
        return View(dto);
    }

    public async Task<IActionResult> EditFabricGroup(int id)
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        var g = res.Data?.FirstOrDefault(x => x.Id == id);
        if (g == null) return NotFound();

        var dto = new UpdateFabricGroupDTO
        {
            Name = g.Name,
            Description = g.Description ?? "",
            DisplayOrder = g.DisplayOrder
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFabricGroup(int id, UpdateFabricGroupDTO dto)
    {
        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _fabricApiClient.UpdateFabricGroupAsync(id, dto, token);
            if (res.IsSuccess)
            {
                TempData["Success"] = "Cập nhật nhóm vải thành công";
                return RedirectToAction(nameof(FabricGroups));
            }
            TempData["Error"] = res.Message ?? "Không thể cập nhật nhóm vải.";
        }
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFabricGroup(int id)
    {
        var token = GetToken();
        var res = await _fabricApiClient.DeleteFabricGroupAsync(id, token);
        if (res.IsSuccess)
        {
            TempData["Success"] = "Xóa nhóm vải thành công";
        }
        else
        {
            TempData["Error"] = res.Message ?? "Không thể xóa nhóm vải.";
        }
        return RedirectToAction(nameof(FabricGroups));
    }

    public async Task<IActionResult> Fabrics(int? groupId)
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        var groups = MapToGroupDTOs(res.Data);
        var list = new List<FabricDTO>();

        foreach (var g in groups)
        {
            if (!groupId.HasValue || g.Id == groupId.Value)
            {
                if (g.Fabrics != null)
                {
                    list.AddRange(g.Fabrics);
                }
            }
        }

        ViewBag.FabricGroups = groups;
        ViewBag.SelectedGroupId = groupId;
        return View(list);
    }

    public async Task<IActionResult> CreateFabric(int? groupId)
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = MapToGroupDTOs(res.Data);
        return View(new CreateFabricDTO { FabricGroupId = groupId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFabric(CreateFabricDTO dto)
    {
        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _fabricApiClient.CreateFabricAsync(dto, token);
            if (res.IsSuccess)
            {
                TempData["Success"] = "Thêm vải thành công";
                return RedirectToAction(nameof(Fabrics));
            }
            TempData["Error"] = res.Message ?? "Không thể thêm vải.";
        }

        var groupsRes = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = MapToGroupDTOs(groupsRes.Data);
        return View(dto);
    }

    public async Task<IActionResult> EditFabric(int id)
    {
        var res = await _fabricApiClient.GetFabricByIdAsync(id);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var f = res.Data;
        var groupsRes = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = MapToGroupDTOs(groupsRes.Data);
        ViewBag.FabricId = id;

        var dto = new UpdateFabricDTO
        {
            Name = f.Name,
            Composition = f.Composition ?? f.Material ?? "",
            Price = f.Price > 0 ? f.Price : f.PricePerMeter,
            ImageUrl = f.ImageUrl ?? "",
            FabricGroupId = f.FabricGroupId,
            IsAvailable = f.IsAvailable
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFabric(int id, UpdateFabricDTO dto)
    {
        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _fabricApiClient.UpdateFabricAsync(id, dto, token);
            if (res.IsSuccess)
            {
                TempData["Success"] = "Cập nhật vải thành công";
                return RedirectToAction(nameof(Fabrics));
            }
            TempData["Error"] = res.Message ?? "Không thể cập nhật vải.";
        }

        var groupsRes = await _fabricApiClient.GetFabricGroupsAsync();
        ViewBag.FabricGroups = MapToGroupDTOs(groupsRes.Data);
        ViewBag.FabricId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFabric(int id)
    {
        var token = GetToken();
        var res = await _fabricApiClient.DeleteFabricAsync(id, token);
        if (res.IsSuccess)
        {
            TempData["Success"] = "Xóa vải thành công";
        }
        else
        {
            TempData["Error"] = res.Message ?? "Không thể xóa vải.";
        }
        return RedirectToAction(nameof(Fabrics));
    }
}
