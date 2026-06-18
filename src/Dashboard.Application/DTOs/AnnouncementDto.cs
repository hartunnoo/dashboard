using Dashboard.Domain.Enums;

namespace Dashboard.Application.DTOs;

public sealed record AnnouncementDto(
    Guid Id, string Title, string Content, AnnouncementSeverity Severity,
    DateTime CreatedAt, DateTime? ExpiresAt);
