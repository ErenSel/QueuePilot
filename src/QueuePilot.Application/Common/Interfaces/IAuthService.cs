using QueuePilot.Application.Auth.Commands;

namespace QueuePilot.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterCommand command);
    Task<AuthResult> LoginAsync(LoginCommand command);
    Task<AuthResult> RefreshTokenAsync(RefreshStepCommand command);
    Task RevokeTokenAsync(RevokeCommand command);
    Task RevokeCurrentUserAsync(string email);
}
