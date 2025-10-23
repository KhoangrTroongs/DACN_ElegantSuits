using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.Models.ViewModels;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class CustomDesignController : Controller
    {
        private readonly IFabricService _fabricService;
        private readonly IProductService _productService;

        public CustomDesignController(IFabricService fabricService, IProductService productService)
        {
            _fabricService = fabricService;
            _productService = productService;
        }

        // GET: CustomDesign/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                return View(fabricGroups);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: CustomDesign/FabricGroup/{groupId}
        public async Task<IActionResult> FabricGroup(int groupId)
        {
            try
            {
                var fabricGroup = await _fabricService.GetFabricGroupByIdAsync(groupId);
                if (fabricGroup == null)
                {
                    TempData["ErrorMessage"] = "Nhóm vải không tìm thấy";
                    return RedirectToAction("Index");
                }

                return View(fabricGroup);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: CustomDesign/FabricDetail/{fabricId}
        public async Task<IActionResult> FabricDetail(int fabricId)
        {
            try
            {
                var fabric = await _fabricService.GetFabricByIdAsync(fabricId);
                if (fabric == null)
                {
                    TempData["ErrorMessage"] = "Vải không tìm thấy";
                    return RedirectToAction("Index");
                }

                return View(fabric);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: CustomDesign/SelectProduct
        public async Task<IActionResult> SelectProduct()
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(null, includeHidden: false);
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: CustomDesign/DesignProduct/{productId}
        public async Task<IActionResult> DesignProduct(int productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tìm thấy";
                    return RedirectToAction("SelectProduct");
                }

                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();

                // Create a view model to pass both product and fabric groups
                var viewModel = new DesignProductViewModel
                {
                    Product = product,
                    FabricGroups = fabricGroups
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("SelectProduct");
            }
        }
    }
}

