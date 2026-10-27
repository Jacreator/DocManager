using DocManager.Application.Common;
using DocManager.Application.Documents;
using DocManager.Web.Extensions;
using DocManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DocManager.Web.Controllers;

public sealed class DocumentsController(IDocumentService documentService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var result = await documentService.SearchAsync(search, page, ct);
        return View(new DocumentIndexViewModel(search, result.Value!));
    }

    [HttpGet]
    public IActionResult Upload() => View(new UploadDocumentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var content = model.File?.OpenReadStream() ?? Stream.Null;
        var result = await documentService.UploadAsync(model.ToRequest(content), ct);
        if (!result.IsSuccess)
        {
            ModelState.AddResultErrors(result);
            return View(model);
        }

        TempData[TempDataKeys.SuccessMessage] = $"\"{model.Title}\" was uploaded.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await documentService.GetDetailsAsync(id, ct);
        return result.IsSuccess
            ? View(new DocumentDetailsViewModel(result.Value!))
            : NotFound();
    }

    /// <summary>The only way to read stored files: the service applies the company and soft-delete filters.</summary>
    [HttpGet]
    public async Task<IActionResult> File(int id, bool download = false, CancellationToken ct = default)
    {
        var result = await documentService.GetFileAsync(id, ct);
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var file = result.Value!;
        if (download)
        {
            return File(file.Content, file.ContentType, file.FileName);
        }

        Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") { FileNameStar = file.FileName }.ToString();
        return File(file.Content, file.ContentType, enableRangeProcessing: true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await documentService.SoftDeleteAsync(id, ct);
        return RedirectWithMessage(result, "The document was moved to the recycle bin.", nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RecycleBin(int page = 1, CancellationToken ct = default)
    {
        var result = await documentService.GetDeletedAsync(page, ct);
        return View(new RecycleBinViewModel(result.Value!));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        var result = await documentService.RestoreAsync(id, ct);
        return RedirectWithMessage(result, "The document was restored.", nameof(RecycleBin));
    }

    private IActionResult RedirectWithMessage(Result result, string successMessage, string actionName)
    {
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        TempData[TempDataKeys.SuccessMessage] = successMessage;
        return RedirectToAction(actionName);
    }
}
