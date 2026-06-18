using Dashboard.Application.Commands.CreateNote;
using Dashboard.Application.DTOs;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Interfaces;
using MediatR;

namespace Dashboard.Infrastructure.Handlers;

public class CreateNoteHandler(IDashboardNoteRepository repo, IUnitOfWork uow)
    : IRequestHandler<CreateNoteCommand, DashboardNoteDto>
{
    public async Task<DashboardNoteDto> Handle(CreateNoteCommand c, CancellationToken ct)
    {
        var note = new DashboardNote
        {
            Id = Guid.NewGuid(), Content = c.Content,
            CreatedByUserId = c.CreatedByUserId, CreatedByName = c.CreatedByName,
            StartsAt = DateTime.UtcNow, ExpiresAt = c.ExpiresAt
        };
        await repo.AddAsync(note, ct);
        await uow.SaveChangesAsync(ct);
        return new DashboardNoteDto(note.Id, note.Content, note.CreatedByUserId,
            note.CreatedByName, note.CreatedAt, note.StartsAt, note.ExpiresAt, note.IsActive);
    }
}
