using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NgoHuuDuc_2280600725.Responsitories;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [ApiController]
    [Route("api/model3d")]
    public class Model3DController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Model3DController> _logger;
        private readonly IConfiguration _config;

        public Model3DController(
            IProductRepository productRepository,
            IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory,
            ILogger<Model3DController> logger,
            IConfiguration config)
        {
            _productRepository = productRepository;
            _env = env;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config;
        }

        public class StatusResponse
        {
            public bool Ready { get; set; }
            public string? Model3DUrl { get; set; }
        }

        [HttpGet("status/{productId:int}")]
        public async Task<IActionResult> GetStatus([FromRoute] int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return NotFound();
            return Ok(new StatusResponse
            {
                Ready = !string.IsNullOrWhiteSpace(product.Model3DUrl),
                Model3DUrl = product.Model3DUrl
            });
        }

        public class CallbackRequest
        {
            public bool Success { get; set; }
            public int ProductId { get; set; }
            public string? Model3DUrl { get; set; }
            public string? ModelUrl { get; set; }
            public string? FileName { get; set; }
            public string? Status { get; set; }
            public string? Secret { get; set; }
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromBody] CallbackRequest req)
        {
            if (req == null || req.ProductId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid payload" });
            }

            // Optional: verify shared secret if configured
            var expectedSecret = _config["N8N:CallbackSecret"];
            if (!string.IsNullOrEmpty(expectedSecret))
            {
                if (!string.Equals(req.Secret, expectedSecret, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Model3D callback rejected due to invalid secret. ProductId={ProductId}", req.ProductId);
                    return Unauthorized(new { success = false, message = "Invalid secret" });
                }
            }

            // Check if generation was successful
            if (!req.Success)
            {
                _logger.LogWarning("Model3D generation failed for product {ProductId}", req.ProductId);
                return Ok(new { success = false, message = "Generation failed" });
            }

            // Get model URL from either Model3DUrl or ModelUrl
            var modelUrl = req.Model3DUrl ?? req.ModelUrl;
            if (string.IsNullOrWhiteSpace(modelUrl))
            {
                return BadRequest(new { success = false, message = "No model URL provided" });
            }

            var product = await _productRepository.GetProductByIdAsync(req.ProductId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var bytes = await client.GetByteArrayAsync(modelUrl);

                var uploadsFolder = Path.Combine(_env.WebRootPath, "models", "products");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(req.FileName ?? modelUrl)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || (ext != ".glb" && ext != ".gltf"))
                {
                    ext = ".glb"; // default to glb
                }

                var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                var localUrl = $"/models/products/{uniqueFileName}";
                product.Model3DUrl = localUrl;
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation("Saved 3D model for product {ProductId} to {LocalUrl}", req.ProductId, localUrl);
                return Ok(new { success = true, localUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Model3D callback for product {ProductId}", req.ProductId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class StartRequest
        {
            public string? WebhookUrl { get; set; }
        }

        [HttpPost("start/{productId:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Start([FromRoute] int productId, [FromBody] StartRequest? req)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return NotFound(new { success = false, message = "Product not found" });
            if (string.IsNullOrWhiteSpace(product.ImageUrl)) return BadRequest(new { success = false, message = "Product has no image" });

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Download product image
                var absoluteImageUrl = product.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? product.ImageUrl
                    : (Request.Scheme + "://" + Request.Host + product.ImageUrl);

                var imageBytes = await client.GetByteArrayAsync(absoluteImageUrl);

                // Upload to temporary storage (tmpfiles.org)
                var imageUrl = await UploadImageToTempStorage(imageBytes, client);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return StatusCode(500, new { success = false, message = "Failed to upload image to temporary storage" });
                }

                var callbackSecret = _config["N8N:CallbackSecret"];
                var callbackUrl = (Request.Scheme + "://" + Request.Host + "/api/model3d/callback");

                // New webhook payload format for N8N
                var payload = new
                {
                    imageUrl = imageUrl,
                    productId = product.Id,
                    productName = product.Name,
                    webhookUrl = callbackUrl,
                    secret = callbackSecret
                };

                var webhookUrl = req?.WebhookUrl ?? _config["N8N:GeneratorWebhookUrl"] ?? "http://localhost:5678/webhook/3d-generator-webhook";
                var post = await client.PostAsJsonAsync(webhookUrl, payload);
                var text = await post.Content.ReadAsStringAsync();
                if (!post.IsSuccessStatusCode)
                {
                    _logger.LogWarning("N8N webhook returned status {StatusCode}: {Response}", post.StatusCode, text);
                    return StatusCode((int)post.StatusCode, new { success = false, message = "n8n error", response = text });
                }

                _logger.LogInformation("Started 3D generation for product {ProductId} via webhook", productId);
                return Ok(new { success = true, message = "3D model generation started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting 3D generation for product {ProductId}", productId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private async Task<string?> UploadImageToTempStorage(byte[] imageBytes, HttpClient client)
        {
            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(imageBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "file");

                    var response = await client.PostAsync("https://tmpfiles.org/api/v1/upload", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to upload to tmpfiles.org: {StatusCode}", response.StatusCode);
                        return null;
                    }

                    var responseText = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(responseText))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("data", out var dataElement) &&
                            dataElement.TryGetProperty("url", out var urlElement))
                        {
                            var uploadUrl = urlElement.GetString();
                            // Convert to download URL
                            if (!string.IsNullOrEmpty(uploadUrl))
                            {
                                return uploadUrl.Replace("https://tmpfiles.org/", "https://tmpfiles.org/dl/");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to temporary storage");
            }

            return null;
        }
    }
}

