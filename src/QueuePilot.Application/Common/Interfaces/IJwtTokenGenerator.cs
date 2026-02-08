using QueuePilot.Domain.Entities;

namespace QueuePilot.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken();
}
