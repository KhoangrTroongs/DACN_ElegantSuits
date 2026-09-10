using System.Text.RegularExpressions;
using ElegantSuits.Application.Common.Interfaces;

namespace ElegantSuits.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string? basePath = null)
    {
        _basePath = basePath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var targetFolder = Path.Combine(_basePath, folder.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var rawName = Path.GetFileNameWithoutExtension(fileName);
        var cleanName = Regex.Replace(rawName, @"[^a-zA-Z0-9_-]", "_");

        var uniqueFileName = $"{cleanName}_{DateTime.UtcNow.Ticks}{ext}";
        var fullPath = Path.Combine(targetFolder, uniqueFileName);

        using (var output = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        return $"/{folder.Trim('/')}/{uniqueFileName}";
    }

    public Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.FromResult(false);
        }

        var cleanRelPath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_basePath, cleanRelPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
