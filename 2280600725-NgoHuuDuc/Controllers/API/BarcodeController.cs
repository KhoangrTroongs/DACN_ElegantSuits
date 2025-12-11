using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BarcodeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BarcodeController> _logger;

        public BarcodeController(ApplicationDbContext context, ILogger<BarcodeController> logger)
        {
            _context = context;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportBarcodesPdf()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.LinearCode != null && p.LinearCode != "")
                    .OrderBy(p => p.Id)
                    .ToListAsync();

                if (!products.Any())
                {
                    return BadRequest(new { success = false, message = "Không có sản phẩm nào có mã Linear" });
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(content => ComposeContent(content, products));
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                var fileName = $"Barcodes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất PDF mã vạch");
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Danh Sách Mã Vạch Sản Phẩm").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).Italic();
                });
            });
        }

        private void ComposeContent(IContainer container, List<NgoHuuDuc_2280600725.Models.Product> products)
        {
            container.PaddingVertical(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(4);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Tên Sản Phẩm");
                    header.Cell().Element(CellStyle).Text("Mã Linear");
                    header.Cell().Element(CellStyle).Text("Mã Vạch");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5);
                    }
                });

                foreach (var product in products)
                {
                    table.Cell().Element(CellStyle).Text(product.Id.ToString());
                    table.Cell().Element(CellStyle).Text(product.Name);
                    table.Cell().Element(CellStyle).Text(product.LinearCode);
                    
                    var barcodeBytes = GenerateBarcodeBytes(product.LinearCode);
                    if (barcodeBytes != null)
                    {
                        table.Cell().Element(CellStyle).Column(col => 
                        {
                            col.Item().Height(40).Image(barcodeBytes);
                            col.Item().AlignCenter().Text(product.LinearCode).FontSize(9);
                        });
                    }
                    else
                    {
                        table.Cell().Element(CellStyle).Text("Lỗi tạo mã");
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(5).AlignMiddle();
                    }
                }
            });
        }

        private byte[]? GenerateBarcodeBytes(string content)
        {
            try
            {
                // Sử dụng MultiFormatWriter để tạo BitMatrix (không phụ thuộc platform/bindings)
                var writer = new MultiFormatWriter();
                var bitMatrix = writer.encode(content, BarcodeFormat.CODE_128, 300, 80, new Dictionary<EncodeHintType, object> {
                    { EncodeHintType.MARGIN, 0 }
                });

                // Chuyển BitMatrix sang SKBitmap thủ công
                using var bitmap = new SKBitmap(bitMatrix.Width, bitMatrix.Height);
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    for (int x = 0; x < bitMatrix.Width; x++)
                    {
                        // Nếu bit là true -> màu đen, false -> màu trắng
                        bitmap.SetPixel(x, y, bitMatrix[x, y] ? SKColors.Black : SKColors.White);
                    }
                }

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                
                return data.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating barcode for {content}: {ex.Message}");
                return null;
            }
        }
    }
}
