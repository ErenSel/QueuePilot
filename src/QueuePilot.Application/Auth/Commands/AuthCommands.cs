using QueuePilot.Domain.Enums;

namespace QueuePilot.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password, UserRole Role);
public record LoginCommand(string Email, string Password);
public record RefreshStepCommand(string AccessToken, string RefreshToken);
public record RevokeCommand(string Email);

public record AuthResult(string AccessToken, string RefreshToken);
