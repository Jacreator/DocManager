namespace DocManager.Application.Abstractions;

/// <summary>
/// Stores binary file content by name. Implementations (local disk, Azure Blob, ...)
/// can be swapped in DI without changing the services that use them.
/// </summary>
public interface IFileStorage
{
    Task SaveAsync(Stream content, string fileName, CancellationToken ct = default);

    /// <summary>Opens the file for reading, or returns null when it does not exist.</summary>
    Task<Stream?> OpenReadAsync(string fileName, CancellationToken ct = default);

    Task DeleteAsync(string fileName, CancellationToken ct = default);
}
