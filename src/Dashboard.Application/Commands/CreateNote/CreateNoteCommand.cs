using Dashboard.Application.DTOs;
using MediatR;

namespace Dashboard.Application.Commands.CreateNote;

public sealed record CreateNoteCommand(
    string Content, string CreatedByUserId, string CreatedByName,
    DateTime? ExpiresAt) : IRequest<DashboardNoteDto>;
