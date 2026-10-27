namespace DocManager.Infrastructure.Storage;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Absolute path, or a path relative to the app's content root. Must be outside wwwroot.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";
}
