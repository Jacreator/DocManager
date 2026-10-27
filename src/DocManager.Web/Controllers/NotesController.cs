using DocManager.Application.Notes;
using DocManager.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DocManager.Web.Controllers;

[ApiController]
[Route("Documents/{documentId:int}/Notes")]
public sealed class NotesController(INoteService noteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NoteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<NoteDto>>> GetNotes(int documentId, CancellationToken ct)
    {
        var result = await noteService.GetNotesAsync(documentId, ct);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    /// <summary>Called via fetch(); the anti-forgery token is read from the request header.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<NoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteDto>> Create(int documentId, CreateNoteRequest request, CancellationToken ct)
    {
        var result = await noteService.AddNoteAsync(documentId, request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetNotes), new { documentId }, result.Value)
            : this.ToProblem(result);
    }
}
