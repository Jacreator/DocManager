using DocManager.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DocManager.Infrastructure.Storage;

public sealed class LocalFileStorage(IOptions<LocalFileStorageOptions> options, IHostEnvironment environment)
    : IFileStorage
{
    private const int BufferSize = 81920;

    private readonly string _rootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RootPath));

    public async Task SaveAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_rootPath);

        await using var target = new FileStream(
            GetSafePath(fileName), FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);
        await content.CopyToAsync(target, ct);
    }

    public Task<Stream?> OpenReadAsync(string fileName, CancellationToken ct = default)
    {
        var path = GetSafePath(fileName);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        File.Delete(GetSafePath(fileName));
        return Task.CompletedTask;
    }

    /// <summary>Rejects anything that is not a plain file name, preventing path traversal.</summary>
    private string GetSafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
        {
            throw new ArgumentException($"'{fileName}' is not a valid stored file name.", nameof(fileName));
        }

        return Path.Combine(_rootPath, fileName);
    }
}
