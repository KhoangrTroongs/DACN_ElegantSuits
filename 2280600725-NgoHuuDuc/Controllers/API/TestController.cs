using Microsoft.AspNetCore.Mvc;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Trả về một JSON đơn giản để kiểm tra API hoạt động
            return Ok(new { message = "API is working!" });
        }
    }
}
