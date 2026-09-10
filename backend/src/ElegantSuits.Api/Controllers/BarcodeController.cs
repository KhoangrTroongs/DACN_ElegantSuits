using ElegantSuits.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BarcodeController : ControllerBase
{
    private readonly IBarcodeService _barcodeService;

    public BarcodeController(IBarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportBarcodesPdf(CancellationToken cancellationToken)
    {
        var pdfBytes = await _barcodeService.ExportBarcodesPdfAsync(cancellationToken);
        var fileName = $"Barcodes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
