namespace DocManager.Web.Extensions;

public static class HttpRequestExtensions
{
    private const string JsonMediaType = "application/json";

    /// <summary>True for AJAX/API calls that expect or send JSON rather than an HTML page.</summary>
    public static bool IsJsonRequest(this HttpRequest request) =>
        request.Headers.Accept.ToString().Contains(JsonMediaType, StringComparison.OrdinalIgnoreCase)
        || request.HasJsonContentType();
}
