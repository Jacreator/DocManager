namespace DocManager.Application.Documents;

public sealed record UploadDocumentRequest(
    string Title,
    string FileName,
    long Length,
    Stream Content);
