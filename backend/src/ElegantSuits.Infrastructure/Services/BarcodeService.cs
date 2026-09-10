using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace ElegantSuits.Infrastructure.Services;

public class BarcodeService : IBarcodeService
{
    private readonly ApplicationDbContext _context;

    public BarcodeService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[]? GenerateBarcodeBytes(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        try
        {
            var writer = new MultiFormatWriter();
            var bitMatrix = writer.encode(content, BarcodeFormat.CODE_128, 300, 80, new Dictionary<EncodeHintType, object>
            {
                { EncodeHintType.MARGIN, 0 }
            });

            using var bitmap = new SKBitmap(bitMatrix.Width, bitMatrix.Height);
            for (int y = 0; y < bitMatrix.Height; y++)
            {
                for (int x = 0; x < bitMatrix.Width; x++)
                {
                    bitmap.SetPixel(x, y, bitMatrix[x, y] ? SKColors.Black : SKColors.White);
                }
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]> ExportBarcodesPdfAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.LinearCode != null && p.LinearCode != "")
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Danh Sách Mã Vạch Sản Phẩm").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        column.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).Italic();
                    });
                });

                page.Content().PaddingVertical(10).Table(table =>
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

                        static IContainer CellStyle(IContainer c) =>
                            c.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5);
                    });

                    foreach (var product in products)
                    {
                        table.Cell().Element(CellStyle).Text(product.Id.ToString());
                        table.Cell().Element(CellStyle).Text(product.Name);
                        table.Cell().Element(CellStyle).Text(product.LinearCode ?? "");

                        var barcodeBytes = GenerateBarcodeBytes(product.LinearCode ?? "");
                        if (barcodeBytes != null)
                        {
                            table.Cell().Element(CellStyle).Column(col =>
                            {
                                col.Item().Height(40).Image(barcodeBytes);
                                col.Item().AlignCenter().Text(product.LinearCode ?? "").FontSize(9);
                            });
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text("N/A");
                        }

                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(5).AlignMiddle();
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
