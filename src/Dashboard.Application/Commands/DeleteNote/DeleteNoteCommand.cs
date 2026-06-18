using MediatR;

namespace Dashboard.Application.Commands.DeleteNote;

public sealed record DeleteNoteCommand(Guid NoteId) : IRequest;
