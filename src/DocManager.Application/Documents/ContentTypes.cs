namespace DocManager.Application.Documents;

/// <summary>
/// Maps file extensions to content types on the server, so the browser-supplied
/// Content-Type header is never trusted.
/// </summary>
internal static class ContentTypes
{
    private const string Fallback = "application/octet-stream";

    private static readonly Dictionary<string, string> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    public static string FromFileName(string fileName) =>
        ByExtension.GetValueOrDefault(Path.GetExtension(fileName), Fallback);
}
