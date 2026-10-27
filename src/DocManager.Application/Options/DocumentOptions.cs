namespace DocManager.Application.Options;

public sealed class DocumentOptions
{
    public const string SectionName = "Documents";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Bound from configuration; the binder appends to arrays, so there is no default list here.</summary>
    public string[] AllowedExtensions { get; set; } = [];

    public int PageSize { get; set; } = 10;
}
