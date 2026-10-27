using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DocManager.Application.Notes;

namespace DocManager.Tests.Integration;

public sealed partial class NotesEndpointTests(DocManagerWebApplicationFactory factory)
    : IClassFixture<DocManagerWebApplicationFactory>
{
    private const int SeededDocumentId = 1;
    private const string AntiforgeryHeader = "X-CSRF-TOKEN";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostNote_ValidRequest_Returns201WithCreatedNote()
    {
        await AddAntiforgeryHeaderAsync();

        var response = await _client.PostAsJsonAsync(NotesUrl(SeededDocumentId), new CreateNoteRequest("Please review", 12.5m, 40m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<NoteDto>();
        Assert.NotNull(note);
        Assert.True(note.Id > 0);
        Assert.Equal(("Please review", 12.5m, 40m), (note.NoteText, note.PositionTop, note.PositionLeft));

        var notes = await _client.GetFromJsonAsync<List<NoteDto>>(NotesUrl(SeededDocumentId));
        Assert.Contains(notes!, n => n.Id == note.Id);
    }

    [Fact]
    public async Task PostNote_InvalidRequest_Returns400()
    {
        await AddAntiforgeryHeaderAsync();

        var response = await _client.PostAsJsonAsync(NotesUrl(SeededDocumentId), new CreateNoteRequest("", 150m, 40m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task PostNote_UnknownDocument_Returns404()
    {
        await AddAntiforgeryHeaderAsync();

        var response = await _client.PostAsJsonAsync(NotesUrl(9999), new CreateNoteRequest("Hello", 10m, 10m));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostNote_WithoutAntiforgeryToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync(NotesUrl(SeededDocumentId), new CreateNoteRequest("Hello", 10m, 10m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Index_WithSearch_ShowsOnlyMatchingDocuments()
    {
        var html = await _client.GetStringAsync("/Documents?search=Handbook");

        Assert.Contains("Employee Handbook", html);
        Assert.DoesNotContain("Quarterly Report Q1", html);
    }

    private static string NotesUrl(int documentId) => $"/Documents/{documentId}/Notes";

    /// <summary>Loads the details page (which sets the anti-forgery cookie) and sends its token as a header, like fetchJson does.</summary>
    private async Task AddAntiforgeryHeaderAsync()
    {
        var html = await _client.GetStringAsync($"/Documents/Details/{SeededDocumentId}");
        var token = CsrfMetaTag().Match(html).Groups["token"].Value;
        Assert.False(string.IsNullOrEmpty(token), "The anti-forgery meta tag was not rendered.");

        _client.DefaultRequestHeaders.Remove(AntiforgeryHeader);
        _client.DefaultRequestHeaders.Add(AntiforgeryHeader, token);
    }

    [GeneratedRegex("<meta name=\"csrf-token\" content=\"(?<token>[^\"]+)\"")]
    private static partial Regex CsrfMetaTag();
}
