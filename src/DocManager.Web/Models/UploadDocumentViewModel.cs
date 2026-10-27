using System.ComponentModel.DataAnnotations;
using DocManager.Application.Documents;

namespace DocManager.Web.Models;

/// <summary>
/// Form model for uploads. Rules live in <see cref="UploadDocumentRequestValidator"/> so they are defined once.
/// </summary>
public sealed class UploadDocumentViewModel
{
    public string? Title { get; set; }

    [Display(Name = "File")]
    public IFormFile? File { get; set; }

    public UploadDocumentRequest ToRequest(Stream content) =>
        new(Title ?? string.Empty, File?.FileName ?? string.Empty, File?.Length ?? 0, content);
}
