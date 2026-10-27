using System.Diagnostics;
using DocManager.Web.Extensions;
using DocManager.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocManager.Web.Controllers;

/// <summary>Target of the exception handler and status code page re-execution.</summary>
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult Index() => ErrorView();

    [Route("Error/{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        if (Request.IsJsonRequest())
        {
            return Problem(statusCode: statusCode);
        }

        return statusCode == StatusCodes.Status404NotFound ? View("NotFound") : ErrorView();
    }

    private ViewResult ErrorView() =>
        View("Error", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
}
