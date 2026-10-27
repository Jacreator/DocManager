using FluentValidation.Results;

namespace DocManager.Application.Common;

internal static class ValidationExtensions
{
    public static IEnumerable<string> ErrorMessages(this ValidationResult result) =>
        result.Errors.Select(e => e.ErrorMessage);
}
