using DocManager.Application.Abstractions;
using DocManager.Application.Common;
using DocManager.Application.Options;
using DocManager.Domain.Common;
using DocManager.Domain.Entities;
using DocManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocManager.Application.Documents;

public sealed class DocumentService(
    IDocumentRepository documents,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    ICurrentCompany currentCompany,
    IValidator<UploadDocumentRequest> validator,
    IOptions<DocumentOptions> options,
    ILogger<DocumentService> logger) : IDocumentService
{
    private readonly int _pageSize = options.Value.PageSize;

    public async Task<Result<int>> UploadAsync(UploadDocumentRequest request, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationError(validation.ErrorMessages());
        }

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(request.FileName).ToLowerInvariant()}";
        await fileStorage.SaveAsync(request.Content, storedFileName, ct);

        var document = new Document
        {
            CompanyId = currentCompany.CompanyId,
            Title = request.Title.Trim(),
            FileName = Path.GetFileName(request.FileName),
            StoredFileName = storedFileName,
            ContentType = ContentTypes.FromFileName(request.FileName),
            FileSize = request.Length
        };

        try
        {
            await documents.AddAsync(document, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Saving document {StoredFileName} failed; removing the orphaned file", storedFileName);
            await fileStorage.DeleteAsync(storedFileName, CancellationToken.None);
            throw;
        }

        logger.LogInformation("Uploaded document {DocumentId} ({FileName}, {FileSize} bytes) for company {CompanyId}",
            document.Id, document.FileName, document.FileSize, document.CompanyId);

        return Result<int>.Success(document.Id);
    }

    public async Task<Result<PagedResult<DocumentSummaryDto>>> SearchAsync(string? title, int page, CancellationToken ct = default)
    {
        var results = await documents.SearchAsync(title?.Trim(), NormalizePage(page), _pageSize, ct);
        return Result<PagedResult<DocumentSummaryDto>>.Success(results.Map(d => d.ToSummaryDto()));
    }

    public async Task<Result<DocumentDetailsDto>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var document = await documents.GetWithNotesAsync(id, ct);
        return document is null
            ? Result<DocumentDetailsDto>.NotFound(DocumentNotFound(id))
            : Result<DocumentDetailsDto>.Success(document.ToDetailsDto());
    }

    public async Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var document = await documents.GetByIdAsync(id, ct);
        if (document is null)
        {
            return Result.NotFound(DocumentNotFound(id));
        }

        document.SoftDelete();
        documents.Update(document);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Soft deleted document {DocumentId}", id);
        return Result.Success();
    }

    public async Task<Result<PagedResult<DocumentSummaryDto>>> GetDeletedAsync(int page, CancellationToken ct = default)
    {
        var results = await documents.GetDeletedAsync(NormalizePage(page), _pageSize, ct);
        return Result<PagedResult<DocumentSummaryDto>>.Success(results.Map(d => d.ToSummaryDto()));
    }

    public async Task<Result> RestoreAsync(int id, CancellationToken ct = default)
    {
        var document = await documents.GetDeletedByIdAsync(id, ct);
        if (document is null)
        {
            return Result.NotFound(DocumentNotFound(id));
        }

        document.Restore();
        documents.Update(document);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Restored document {DocumentId}", id);
        return Result.Success();
    }

    public async Task<Result<FileDownloadDto>> GetFileAsync(int id, CancellationToken ct = default)
    {
        var document = await documents.GetByIdAsync(id, ct);
        if (document is null)
        {
            return Result<FileDownloadDto>.NotFound(DocumentNotFound(id));
        }

        var content = await fileStorage.OpenReadAsync(document.StoredFileName, ct);
        if (content is null)
        {
            logger.LogWarning("File {StoredFileName} for document {DocumentId} is missing from storage",
                document.StoredFileName, id);
            return Result<FileDownloadDto>.NotFound($"The file for document {id} could not be found.");
        }

        return Result<FileDownloadDto>.Success(new FileDownloadDto(content, document.ContentType, document.FileName));
    }

    private static int NormalizePage(int page) => Math.Max(page, 1);

    private static string DocumentNotFound(int id) => $"Document {id} was not found.";
}
