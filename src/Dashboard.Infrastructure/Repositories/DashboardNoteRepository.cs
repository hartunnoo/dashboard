using Dashboard.Domain.Entities;
using Dashboard.Domain.Interfaces;
using Dashboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Infrastructure.Repositories;

public class DashboardNoteRepository(DashboardDbContext ctx) : IDashboardNoteRepository
{
    public async Task<IReadOnlyList<DashboardNote>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await ctx.DashboardNotes
            .AsNoTracking()
            .Where(n => n.IsActive && (n.ExpiresAt == null || n.ExpiresAt > now))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<DashboardNote?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await ctx.DashboardNotes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(DashboardNote note, CancellationToken ct = default)
        => await ctx.DashboardNotes.AddAsync(note, ct);

    public void Delete(DashboardNote note) => ctx.DashboardNotes.Remove(note);
}
