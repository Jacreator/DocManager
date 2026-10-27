using DocManager.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DocManager.Web.Extensions;

public static class ResultExtensions
{
    /// <summary>Maps a failed result to a ProblemDetails response for API endpoints.</summary>
    public static ActionResult ToProblem(this ControllerBase controller, Result result) => result.Status switch
    {
        ResultStatus.NotFound => controller.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not found.",
            Detail = string.Join(" ", result.Errors)
        }),
        ResultStatus.ValidationError => controller.BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Extensions = { ["errors"] = result.Errors }
        }),
        _ => throw new InvalidOperationException($"Result status {result.Status} is not an error.")
    };

    public static void AddResultErrors(this ModelStateDictionary modelState, Result result)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error);
        }
    }
}
