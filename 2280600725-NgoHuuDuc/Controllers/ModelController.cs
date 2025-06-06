using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class ModelController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ModelController> _logger;

        public ModelController(IWebHostEnvironment webHostEnvironment, ILogger<ModelController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        [Route("model/{*filePath}")]
        public IActionResult GetModel(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                // Nếu không truyền đường dẫn file thì trả về 404
                return NotFound();
            }

            // Làm sạch đường dẫn để tránh tấn công directory traversal
            filePath = filePath.Replace("..", "").Replace("\\", "/").TrimStart('/');

            // Chỉ cho phép truy cập file trong thư mục models/products
            if (!filePath.StartsWith("products/"))
            {
                _logger.LogWarning("Attempted to access file outside of models/products directory: {FilePath}", filePath);
                return NotFound();
            }

            // Ghép đường dẫn vật lý đầy đủ tới file trong wwwroot/models
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "models", filePath);

            // Kiểm tra file có tồn tại không
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Model file not found: {FilePath}", fullPath);
                return NotFound();
            }

            // Xác định content-type trả về dựa vào phần mở rộng file
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".glb" => "model/gltf-binary",
                ".gltf" => "model/gltf+json",
                ".obj" => "text/plain",
                _ => "application/octet-stream"
            };

            // Trả về file vật lý với content-type phù hợp
            return PhysicalFile(fullPath, contentType);
        }
    }
}
