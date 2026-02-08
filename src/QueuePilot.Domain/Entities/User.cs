using QueuePilot.Domain.Common;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // EF Constructor
    private User() { }

    public static User Create(string email, string passwordHash, UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }
    
    public bool RemoveRefreshToken(string token)
    {
        var existingToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (existingToken != null)
        {
             return _refreshTokens.Remove(existingToken);
        }
        return false;
    }
}
