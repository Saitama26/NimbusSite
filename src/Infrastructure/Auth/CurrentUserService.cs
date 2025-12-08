using Application.Abstractions.Auth;
using Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Auth;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Role")?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

