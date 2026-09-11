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

    public IActionResult Index() => RedirectToAction(nameof(FabricGroups));

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

    public async Task<IActionResult> FabricGroups()
    {
        var res = await _fabricApiClient.GetFabricGroupsAsync();
        var groups = MapToGroupDTOs(res.Data);
        return View(groups);
    }

    public IActionResult CreateFabricGroup() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateFabricGroup(CreateFabricGroupDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["Success"] = "Thêm nhóm vải thành công";
            return RedirectToAction(nameof(FabricGroups));
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
    public IActionResult EditFabricGroup(int id, UpdateFabricGroupDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["Success"] = "Cập nhật nhóm vải thành công";
            return RedirectToAction(nameof(FabricGroups));
        }
        return View(dto);
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
    public IActionResult CreateFabric(CreateFabricDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["Success"] = "Thêm vải thành công";
            return RedirectToAction(nameof(Fabrics));
        }
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
    public IActionResult EditFabric(int id, UpdateFabricDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["Success"] = "Cập nhật vải thành công";
            return RedirectToAction(nameof(Fabrics));
        }
        return View(dto);
    }
}
