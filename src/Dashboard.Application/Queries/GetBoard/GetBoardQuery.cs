using Dashboard.Application.DTOs;
using MediatR;

namespace Dashboard.Application.Queries.GetBoard;

public sealed record GetBoardQuery(string UserId, string Roles, bool IsPortfolioManager)
    : IRequest<BoardViewModel>;
