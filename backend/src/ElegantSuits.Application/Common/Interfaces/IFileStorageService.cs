namespace ElegantSuits.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
}
