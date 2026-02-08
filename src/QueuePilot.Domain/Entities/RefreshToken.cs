using QueuePilot.Domain.Common;

namespace QueuePilot.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    
    // EF Constructor
    private RefreshToken() { }

    public static RefreshToken Create(string token, DateTime expiryDate)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiryDate = expiryDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
}
