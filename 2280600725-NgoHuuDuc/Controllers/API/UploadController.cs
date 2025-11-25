using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string type = "products")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn file ảnh" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận jpg, jpeg, png, gif, webp" });
                }

                // Determine folder based on type
                string folderName = "products";
                if (type.ToLower() == "users") folderName = "users";
                if (type.ToLower() == "fabrics") folderName = "catolog"; // Note: folder name in project is 'catolog'

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folderName);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var imageUrl = $"/images/{folderName}/{uniqueFileName}";
                return Ok(new { url = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = "Lỗi khi upload ảnh" });
            }
        }
    }
}
