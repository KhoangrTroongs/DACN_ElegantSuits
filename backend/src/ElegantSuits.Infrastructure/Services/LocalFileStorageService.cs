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

        // Tự động đồng bộ file sang thư mục wwwroot của Frontend Web
        try
        {
            var frontendWwwroot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\frontend\src\ElegantSuits.Web\wwwroot"));
            if (Directory.Exists(frontendWwwroot))
            {
                var frontendFolder = Path.Combine(frontendWwwroot, folder.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(frontendFolder)) Directory.CreateDirectory(frontendFolder);
                var frontendPath = Path.Combine(frontendFolder, uniqueFileName);
                File.Copy(fullPath, frontendPath, true);
            }
        }
        catch
        {
            // Bỏ qua lỗi đồng bộ phụ
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

        bool deleted = false;
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            deleted = true;
        }

        try
        {
            var frontendWwwroot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\frontend\src\ElegantSuits.Web\wwwroot"));
            var frontendPath = Path.Combine(frontendWwwroot, cleanRelPath);
            if (File.Exists(frontendPath))
            {
                File.Delete(frontendPath);
                deleted = true;
            }
        }
        catch { }

        return Task.FromResult(deleted);
    }
}
