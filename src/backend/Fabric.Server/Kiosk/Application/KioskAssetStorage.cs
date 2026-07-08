using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Kiosk.Application;

public interface IKioskAssetStorage
{
    Task<string> SaveAsync(Guid profileId, Guid assetId, string fileName, Stream stream, CancellationToken cancellationToken);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}

public sealed class KioskAssetStorage(IOptions<KioskAssetStorageOptions> options, ITenantContext tenantContext) : IKioskAssetStorage
{
    public async Task<string> SaveAsync(Guid profileId, Guid assetId, string fileName, Stream stream, CancellationToken cancellationToken)
    {
        string safeFileName = Path.GetFileName(fileName);
        string relativePath = Path.Combine(tenantContext.TenantId, profileId.ToString("N"), assetId.ToString("N"), safeFileName);
        string fullPath = GetFullPath(relativePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
            Directory.CreateDirectory(directory);

        await using FileStream fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);
        return relativePath;
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        string fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        string fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string GetFullPath(string relativePath) => Path.GetFullPath(Path.Combine(options.Value.Path, relativePath));
}
