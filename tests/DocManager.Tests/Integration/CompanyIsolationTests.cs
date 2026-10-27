using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DocManager.Application.Notes;

namespace DocManager.Tests.Integration;

/// <summary>
/// Uses the seed data: company 1 owns documents 1-3; company 2 owns 1001-1003 (a note on 1001)
/// and has document 1004 in its recycle bin.
/// </summary>
public sealed partial class CompanyIsolationTests(DocManagerWebApplicationFactory factory)
    : IClassFixture<DocManagerWebApplicationFactory>
{
    private const int CompanyOneDocumentId = 1;
    private const int CompanyTwoDocumentId = 1001;
    private const int CompanyTwoDeletedDocumentId = 1004;
    private const string AntiforgeryHeader = "X-CSRF-TOKEN";

    private readonly HttpClient _companyOne = factory.CreateClientForCompany(1);
    private readonly HttpClient _companyTwo = factory.CreateClientForCompany(2);

    [Fact]
    public async Task DocumentList_ShowsOnlyTheCurrentCompanysDocuments()
    {
        var companyOneHtml = await _companyOne.GetStringAsync("/Documents");
        var companyTwoHtml = await _companyTwo.GetStringAsync("/Documents");

        Assert.Contains("Employee Handbook", companyOneHtml);
        Assert.DoesNotContain("Supplier Contract", companyOneHtml);

        Assert.Contains("Supplier Contract", companyTwoHtml);
        Assert.DoesNotContain("Employee Handbook", companyTwoHtml);
    }

    [Fact]
    public async Task Search_DoesNotFindOtherCompanysDocuments()
    {
        var html = await _companyOne.GetStringAsync("/Documents?search=Contract");

        Assert.DoesNotContain("Supplier Contract", html);
    }

    [Fact]
    public async Task Details_OfOtherCompanysDocument_Returns404()
    {
        var response = await _companyOne.GetAsync($"/Documents/Details/{CompanyTwoDocumentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Details_OfOwnDocument_ShowsItsNotes()
    {
        var html = await _companyTwo.GetStringAsync($"/Documents/Details/{CompanyTwoDocumentId}");

        Assert.Contains("renegotiate payment terms", html);
    }

    [Fact]
    public async Task Notes_OfOtherCompanysDocument_CannotBeReadOrAdded()
    {
        await AddAntiforgeryHeaderAsync(_companyOne);

        var read = await _companyOne.GetAsync($"/Documents/{CompanyTwoDocumentId}/Notes");
        var add = await _companyOne.PostAsJsonAsync($"/Documents/{CompanyTwoDocumentId}/Notes", new CreateNoteRequest("Snooping", 10m, 10m));

        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, add.StatusCode);
    }

    [Fact]
    public async Task Delete_OfOtherCompanysDocument_Returns404AndLeavesItInPlace()
    {
        await AddAntiforgeryHeaderAsync(_companyOne);

        var response = await _companyOne.PostAsync($"/Documents/Delete/{CompanyTwoDocumentId}", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Supplier Contract", await _companyTwo.GetStringAsync("/Documents"));
    }

    [Fact]
    public async Task RecycleBin_ShowsOnlyTheCurrentCompanysDeletedDocuments()
    {
        var companyOneHtml = await _companyOne.GetStringAsync("/Documents/RecycleBin");
        var companyTwoHtml = await _companyTwo.GetStringAsync("/Documents/RecycleBin");

        Assert.DoesNotContain("Price List 2025", companyOneHtml);
        Assert.Contains("Price List 2025", companyTwoHtml);
    }

    [Fact]
    public async Task Restore_OfOtherCompanysDeletedDocument_Returns404()
    {
        await AddAntiforgeryHeaderAsync(_companyOne);

        var response = await _companyOne.PostAsync($"/Documents/Restore/{CompanyTwoDeletedDocumentId}", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Price List 2025", await _companyTwo.GetStringAsync("/Documents/RecycleBin"));
    }

    [Fact]
    public async Task File_UploadedByCompanyTwo_CanOnlyBeDownloadedByCompanyTwo()
    {
        var documentId = await UploadAsync(_companyTwo, "Company Two Private Scan");

        var ownerResponse = await _companyTwo.GetAsync($"/Documents/File/{documentId}");
        var otherResponse = await _companyOne.GetAsync($"/Documents/File/{documentId}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
    }

    [Fact]
    public async Task Details_OfOwnCompanyDocument_IsNotVisibleToCompanyTwo()
    {
        var response = await _companyTwo.GetAsync($"/Documents/Details/{CompanyOneDocumentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<int> UploadAsync(HttpClient client, string title)
    {
        await AddAntiforgeryHeaderAsync(client);

        using var form = new MultipartFormDataContent
        {
            { new StringContent(title), "Title" },
            { new ByteArrayContent("%PDF-1.4 test"u8.ToArray()), "File", "scan.pdf" }
        };
        var response = await client.PostAsync("/Documents/Upload", form);

        // The test client follows the redirect to /Documents/Details/{id}.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return int.Parse(response.RequestMessage!.RequestUri!.Segments[^1]);
    }

    /// <summary>Loads a page (setting the anti-forgery cookie) and sends its token as a header.</summary>
    private static async Task AddAntiforgeryHeaderAsync(HttpClient client)
    {
        var html = await client.GetStringAsync("/Documents");
        var token = CsrfMetaTag().Match(html).Groups["token"].Value;
        Assert.False(string.IsNullOrEmpty(token), "The anti-forgery meta tag was not rendered.");

        client.DefaultRequestHeaders.Remove(AntiforgeryHeader);
        client.DefaultRequestHeaders.Add(AntiforgeryHeader, token);
    }

    [GeneratedRegex("<meta name=\"csrf-token\" content=\"(?<token>[^\"]+)\"")]
    private static partial Regex CsrfMetaTag();
}
