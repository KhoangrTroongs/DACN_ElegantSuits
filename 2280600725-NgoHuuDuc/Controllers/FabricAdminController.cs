using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.IO;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class FabricAdminController : Controller
    {
        private readonly IFabricService _fabricService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FabricAdminController> _logger;

        public FabricAdminController(
            IFabricService fabricService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<FabricAdminController> logger)
        {
            _fabricService = fabricService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // ==================== FABRIC GROUPS ====================

        // GET: FabricAdmin/FabricGroups
        public async Task<IActionResult> FabricGroups()
        {
            try
            {
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                return View(fabricGroups.OrderBy(g => g.DisplayOrder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhóm vải");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách nhóm vải.";
                return View(new List<FabricGroupDTO>());
            }
        }

        // GET: FabricAdmin/CreateFabricGroup
        public IActionResult CreateFabricGroup()
        {
            return View();
        }

        // POST: FabricAdmin/CreateFabricGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFabricGroup(CreateFabricGroupDTO createFabricGroupDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createFabricGroupDTO);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(createFabricGroupDTO.Name))
                {
                    ModelState.AddModelError("Name", "Tên nhóm vải không được để trống");
                    return View(createFabricGroupDTO);
                }

                await _fabricService.AddFabricGroupAsync(createFabricGroupDTO);
                TempData["Success"] = "Thêm nhóm vải thành công";
                return RedirectToAction(nameof(FabricGroups));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm nhóm vải");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm nhóm vải.");
                return View(createFabricGroupDTO);
            }
        }

        // GET: FabricAdmin/EditFabricGroup/5
        public async Task<IActionResult> EditFabricGroup(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var fabricGroup = await _fabricService.GetFabricGroupByIdAsync(id.Value);
                if (fabricGroup == null)
                    return NotFound();

                var updateDTO = new UpdateFabricGroupDTO
                {
                    Name = fabricGroup.Name,
                    Description = fabricGroup.Description,
                    DisplayOrder = fabricGroup.DisplayOrder
                };

                ViewBag.FabricGroupId = id;
                return View(updateDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin nhóm vải");
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin nhóm vải.";
                return RedirectToAction(nameof(FabricGroups));
            }
        }

        // POST: FabricAdmin/EditFabricGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFabricGroup(int id, UpdateFabricGroupDTO updateFabricGroupDTO)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FabricGroupId = id;
                return View(updateFabricGroupDTO);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(updateFabricGroupDTO.Name))
                {
                    ModelState.AddModelError("Name", "Tên nhóm vải không được để trống");
                    ViewBag.FabricGroupId = id;
                    return View(updateFabricGroupDTO);
                }

                await _fabricService.UpdateFabricGroupAsync(id, updateFabricGroupDTO);
                TempData["Success"] = "Cập nhật nhóm vải thành công";
                return RedirectToAction(nameof(FabricGroups));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật nhóm vải");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật nhóm vải.");
                ViewBag.FabricGroupId = id;
                return View(updateFabricGroupDTO);
            }
        }

        // POST: FabricAdmin/DeleteFabricGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFabricGroup(int id)
        {
            try
            {
                await _fabricService.DeleteFabricGroupAsync(id);
                TempData["Success"] = "Xóa nhóm vải thành công";
                return RedirectToAction(nameof(FabricGroups));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhóm vải");
                TempData["Error"] = "Có lỗi xảy ra khi xóa nhóm vải.";
                return RedirectToAction(nameof(FabricGroups));
            }
        }

        // ==================== FABRICS ====================

        // GET: FabricAdmin/Fabrics
        public async Task<IActionResult> Fabrics(int? groupId)
        {
            try
            {
                IEnumerable<FabricDTO> fabrics;
                if (groupId.HasValue)
                {
                    fabrics = await _fabricService.GetFabricsByGroupAsync(groupId.Value);
                }
                else
                {
                    fabrics = await _fabricService.GetAllFabricsAsync();
                }

                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                ViewBag.FabricGroups = fabricGroups;
                ViewBag.SelectedGroupId = groupId;

                return View(fabrics.OrderBy(f => f.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách vải");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách vải.";
                return View(new List<FabricDTO>());
            }
        }

        // GET: FabricAdmin/CreateFabric
        public async Task<IActionResult> CreateFabric()
        {
            try
            {
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                ViewBag.FabricGroups = fabricGroups;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách nhóm vải");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách nhóm vải.";
                return RedirectToAction(nameof(Fabrics));
            }
        }

        // POST: FabricAdmin/CreateFabric
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFabric(CreateFabricDTO createFabricDTO, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                    ViewBag.FabricGroups = fabricGroups;
                    return View(createFabricDTO);
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    createFabricDTO.ImageUrl = await SaveImageAsync(imageFile);
                }
                else
                {
                    createFabricDTO.ImageUrl = "/images/fabrics/default-fabric.jpg";
                }

                await _fabricService.AddFabricAsync(createFabricDTO);
                TempData["Success"] = "Thêm vải thành công";
                return RedirectToAction(nameof(Fabrics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm vải");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm vải.");
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                ViewBag.FabricGroups = fabricGroups;
                return View(createFabricDTO);
            }
        }

        // GET: FabricAdmin/EditFabric/5
        public async Task<IActionResult> EditFabric(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var fabric = await _fabricService.GetFabricByIdAsync(id.Value);
                if (fabric == null)
                    return NotFound();

                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                ViewBag.FabricGroups = fabricGroups;
                ViewBag.FabricId = id;

                var updateDTO = new UpdateFabricDTO
                {
                    Name = fabric.Name,
                    Description = fabric.Description,
                    Composition = fabric.Composition,
                    ImageUrl = fabric.ImageUrl,
                    Price = fabric.Price,
                    FabricGroupId = fabric.FabricGroupId,
                    IsAvailable = fabric.IsAvailable
                };

                return View(updateDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin vải");
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin vải.";
                return RedirectToAction(nameof(Fabrics));
            }
        }

        // POST: FabricAdmin/EditFabric/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFabric(int id, UpdateFabricDTO updateFabricDTO, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                    ViewBag.FabricGroups = fabricGroups;
                    ViewBag.FabricId = id;
                    return View(updateFabricDTO);
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    updateFabricDTO.ImageUrl = await SaveImageAsync(imageFile);
                }

                await _fabricService.UpdateFabricAsync(id, updateFabricDTO);
                TempData["Success"] = "Cập nhật vải thành công";
                return RedirectToAction(nameof(Fabrics));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật vải");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật vải.");
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                ViewBag.FabricGroups = fabricGroups;
                ViewBag.FabricId = id;
                return View(updateFabricDTO);
            }
        }

        // POST: FabricAdmin/DeleteFabric/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFabric(int id)
        {
            try
            {
                await _fabricService.DeleteFabricAsync(id);
                TempData["Success"] = "Xóa vải thành công";
                return RedirectToAction(nameof(Fabrics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa vải");
                TempData["Error"] = "Có lỗi xảy ra khi xóa vải.";
                return RedirectToAction(nameof(Fabrics));
            }
        }

        // Helper method to save image
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "fabrics");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/images/fabrics/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu hình ảnh");
                throw;
            }
        }
    }
}

