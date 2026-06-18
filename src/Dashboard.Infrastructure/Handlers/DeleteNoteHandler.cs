using Dashboard.Application.Commands.DeleteNote;
using Dashboard.Domain.Interfaces;
using MediatR;

namespace Dashboard.Infrastructure.Handlers;

public class DeleteNoteHandler(IDashboardNoteRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteNoteCommand>
{
    public async Task Handle(DeleteNoteCommand c, CancellationToken ct)
    {
        var n = await repo.GetByIdAsync(c.NoteId, ct) ?? throw new KeyNotFoundException();
        repo.Delete(n);
        await uow.SaveChangesAsync(ct);
    }
}
