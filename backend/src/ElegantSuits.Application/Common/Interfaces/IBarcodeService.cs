namespace ElegantSuits.Application.Common.Interfaces;

public interface IBarcodeService
{
    byte[]? GenerateBarcodeBytes(string content);
    Task<byte[]> ExportBarcodesPdfAsync(CancellationToken cancellationToken = default);
}
