using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class FabricsController : ControllerBase
    {
        private readonly IFabricService _fabricService;

        public FabricsController(IFabricService fabricService)
        {
            _fabricService = fabricService;
        }

        // GET: api/fabrics/groups
        [HttpGet("groups")]
        public async Task<ActionResult<IEnumerable<FabricGroupDTO>>> GetAllFabricGroups()
        {
            try
            {
                var fabricGroups = await _fabricService.GetAllFabricGroupsAsync();
                return Ok(fabricGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách nhóm vải", error = ex.Message });
            }
        }

        // GET: api/fabrics/groups/{id}
        [HttpGet("groups/{id}")]
        public async Task<ActionResult<FabricGroupDTO>> GetFabricGroupById(int id)
        {
            try
            {
                var fabricGroup = await _fabricService.GetFabricGroupByIdAsync(id);
                if (fabricGroup == null)
                    return NotFound(new { message = "Nhóm vải không tìm thấy" });

                return Ok(fabricGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin nhóm vải", error = ex.Message });
            }
        }

        // GET: api/fabrics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FabricDTO>>> GetAllFabrics()
        {
            try
            {
                var fabrics = await _fabricService.GetAllFabricsAsync();
                return Ok(fabrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách vải", error = ex.Message });
            }
        }

        // GET: api/fabrics/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FabricDTO>> GetFabricById(int id)
        {
            try
            {
                var fabric = await _fabricService.GetFabricByIdAsync(id);
                if (fabric == null)
                    return NotFound(new { message = "Vải không tìm thấy" });

                return Ok(fabric);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin vải", error = ex.Message });
            }
        }

        // GET: api/fabrics/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<FabricDTO>>> GetFabricsByGroup(int groupId)
        {
            try
            {
                var fabrics = await _fabricService.GetFabricsByGroupAsync(groupId);
                return Ok(fabrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách vải theo nhóm", error = ex.Message });
            }
        }

        // GET: api/fabrics/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<FabricDTO>>> GetFabricsByProduct(int productId)
        {
            try
            {
                var fabrics = await _fabricService.GetFabricsByProductIdAsync(productId);
                return Ok(fabrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách vải của sản phẩm", error = ex.Message });
            }
        }

        // POST: api/fabrics
        [HttpPost]
        public async Task<ActionResult<FabricDTO>> CreateFabric([FromBody] CreateFabricDTO createFabricDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var fabric = await _fabricService.AddFabricAsync(createFabricDTO);
                return CreatedAtAction(nameof(GetFabricById), new { id = fabric.Id }, fabric);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo vải mới", error = ex.Message });
            }
        }

        // PUT: api/fabrics/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<FabricDTO>> UpdateFabric(int id, [FromBody] UpdateFabricDTO updateFabricDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var fabric = await _fabricService.UpdateFabricAsync(id, updateFabricDTO);
                return Ok(fabric);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Vải không tìm thấy" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật vải", error = ex.Message });
            }
        }

        // DELETE: api/fabrics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFabric(int id)
        {
            try
            {
                await _fabricService.DeleteFabricAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa vải", error = ex.Message });
            }
        }

        // POST: api/fabrics/product/{productId}/fabric/{fabricId}
        [HttpPost("product/{productId}/fabric/{fabricId}")]
        public async Task<IActionResult> AddFabricToProduct(int productId, int fabricId)
        {
            try
            {
                await _fabricService.AddFabricToProductAsync(productId, fabricId);
                return Ok(new { message = "Vải đã được thêm vào sản phẩm" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm vải vào sản phẩm", error = ex.Message });
            }
        }

        // DELETE: api/fabrics/product/{productId}/fabric/{fabricId}
        [HttpDelete("product/{productId}/fabric/{fabricId}")]
        public async Task<IActionResult> RemoveFabricFromProduct(int productId, int fabricId)
        {
            try
            {
                await _fabricService.RemoveFabricFromProductAsync(productId, fabricId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa vải khỏi sản phẩm", error = ex.Message });
            }
        }
    }
}

