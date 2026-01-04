using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Identity.Application.Abstractions;
using Identity.Application.Queries.GetSession;
using Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetSession;

/// <summary>
/// Запрос получения сессии по ID
/// </summary>
public sealed record GetSessionQuery(Guid SessionId, int TenantId) : IQuery<SessionDto>;

/// <summary>
/// DTO для сессии (внутренний)
/// </summary>
public sealed record SessionDto(
    Guid Id,
    Guid UserId,
    int TenantId,
    Guid? RefreshTokenId,
    Identity.Domain.Enums.SessionStatus Status,
    string? IpAddress,
    string? UserAgent,
    DateTime LastActivityAt,
    DateTime ExpiresAt,
    DateTime? ClosedAt,
    string? CloseReason,
    DateTime CreatedAt);

/// <summary>
/// Обработчик запроса получения сессии
/// </summary>
internal sealed class GetSessionQueryHandler : IQueryHandler<GetSessionQuery, SessionDto>
{
    private readonly IIdentityDbContext _dbContext;

    public GetSessionQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SessionDto>> Handle(GetSessionQuery query, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.SessionId && s.TenantId == query.TenantId, cancellationToken);
        if (session == null)
        {
            return Result<SessionDto>.Failure(IdentityErrors.SessionNotFound(query.SessionId));
        }

        var dto = new SessionDto(
            session.Id,
            session.UserId,
            session.TenantId,
            session.RefreshTokenId,
            session.Status,
            session.IpAddress,
            session.UserAgent,
            session.LastActivityAt,
            session.ExpiresAt,
            session.ClosedAt,
            session.CloseReason,
            session.CreatedAt);

        return Result<SessionDto>.Success(dto);
    }
}

