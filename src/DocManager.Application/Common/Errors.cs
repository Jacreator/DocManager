namespace DocManager.Application.Common;

/// <summary>Error messages shared by the application services.</summary>
public static class Errors
{
    public static string DocumentNotFound(int id) => $"Document {id} was not found.";

    public static string DocumentFileNotFound(int id) => $"The file for document {id} could not be found.";
}
