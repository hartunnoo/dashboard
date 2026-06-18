using Dashboard.Application.Commands.CreateNote;
using Dashboard.Application.Commands.DeleteNote;
using Dashboard.Application.DTOs;
using Dashboard.Application.Queries.GetBoard;
using Dashboard.Application.Queries.GetNotes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.WebApi.Controllers;

[ApiController]
public class BoardController(IMediator mediator) : Controller
{
    [HttpGet("/")]
    [HttpGet("/board")]
    public async Task<IActionResult> Index(
        [FromHeader(Name = "X-User-Id")] string? userId = null,
        [FromHeader(Name = "X-User-Roles")] string? roles = null)
    {
        userId ??= "anonymous";
        roles ??= "";
        var vm = await mediator.Send(new GetBoardQuery(userId, roles, roles.Contains("Admin")));
        return View("Index", vm);
    }

    [HttpGet("/api/board")]
    public async Task<ActionResult<BoardViewModel>> GetBoard(
        [FromHeader(Name = "X-User-Id")] string? userId = null,
        [FromHeader(Name = "X-User-Roles")] string? roles = null)
    {
        userId ??= "anonymous";
        roles ??= "";
        return Ok(await mediator.Send(new GetBoardQuery(userId, roles, roles.Contains("Admin"))));
    }

    [HttpGet("/api/notes")]
    public async Task<ActionResult<IReadOnlyList<DashboardNoteDto>>> GetNotes(CancellationToken ct)
        => Ok(await mediator.Send(new GetNotesQuery(true), ct));

    [HttpPost("/api/notes")]
    public async Task<ActionResult<DashboardNoteDto>> CreateNote([FromBody] CreateNoteCommand cmd, CancellationToken ct)
        => CreatedAtAction(nameof(GetNotes), null, await mediator.Send(cmd, ct));

    [HttpDelete("/api/notes/{id:guid}")]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteNoteCommand(id), ct);
        return NoContent();
    }
}
