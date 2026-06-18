using Dashboard.Application.DTOs;
using MediatR;

namespace Dashboard.Application.Queries.GetNotes;

public sealed record GetNotesQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<DashboardNoteDto>>;
