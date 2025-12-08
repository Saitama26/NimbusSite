using Domain.Users;
using SharedKernel;

namespace Domain.Auth;

public sealed class RefreshToken : DomainEventEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}

