namespace Dashboard.Application.DTOs;

public sealed record DashboardNoteDto(
    Guid Id, string Content, string CreatedByUserId, string CreatedByName,
    DateTime CreatedAt, DateTime StartsAt, DateTime? ExpiresAt, bool IsActive);
