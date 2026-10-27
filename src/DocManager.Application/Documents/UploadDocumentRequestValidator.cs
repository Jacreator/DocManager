using DocManager.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace DocManager.Application.Documents;

public sealed class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public const int TitleMaxLength = 200;
    public const int FileNameMaxLength = 255;

    public UploadDocumentRequestValidator(IOptions<DocumentOptions> options)
    {
        var settings = options.Value;
        var allowed = string.Join(", ", settings.AllowedExtensions);

        RuleFor(r => r.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TitleMaxLength).WithMessage($"Title must be {TitleMaxLength} characters or fewer.");

        RuleFor(r => r.FileName)
            .NotEmpty().WithMessage("Please choose a file to upload.")
            .MaximumLength(FileNameMaxLength).WithMessage($"File name must be {FileNameMaxLength} characters or fewer.");

        RuleFor(r => r.FileName)
            .Must(name => HasAllowedExtension(name, settings.AllowedExtensions))
            .When(r => !string.IsNullOrWhiteSpace(r.FileName))
            .WithMessage($"Only these file types are allowed: {allowed}.");

        RuleFor(r => r.Length)
            .GreaterThan(0).WithMessage("The file is empty.")
            .LessThanOrEqualTo(settings.MaxFileSizeBytes)
            .WithMessage($"The file must be {settings.MaxFileSizeBytes / 1024 / 1024} MB or smaller.")
            .When(r => !string.IsNullOrWhiteSpace(r.FileName));
    }

    private static bool HasAllowedExtension(string fileName, IEnumerable<string> allowedExtensions) =>
        allowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
}
