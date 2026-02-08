using Microsoft.EntityFrameworkCore;
using QueuePilot.Application.Auth.Commands;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Entities;
using QueuePilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace QueuePilot.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> RegisterAsync(RegisterCommand command)
    {
        if (await _context.Users.AnyAsync(u => u.Email == command.Email))
        {
            throw new Exception("User already exists."); 
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = User.Create(command.Email, passwordHash, command.Role);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateTokensForUser(user);
    }

    public async Task<AuthResult> LoginAsync(LoginCommand command)
    {
        var user = await _context.Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials.");
        }

        return await GenerateTokensForUser(user);
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshStepCommand command)
    {
        // Simple implementation: In real world, validate access token structure too (ignoring expiry)
        
        var user = await _context.Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == command.RefreshToken));

        if (user == null)
            throw new Exception("Invalid token.");

        var existingToken = user.RefreshTokens.First(t => t.Token == command.RefreshToken);

        if (!existingToken.IsActive)
        {
            // Token Reuse Detection could happen here -> Revoke all
            throw new Exception("Invalid token state.");
        }

        existingToken.Revoke(); // Rotate: Invalidate old
        
        return await GenerateTokensForUser(user);
    }
    
    public async Task RevokeTokenAsync(RevokeCommand command)
    {
        var user = await _context.Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == command.Email);
            
        if (user == null) return;
        
        // Revoke all for simplicity or specific logic
        foreach (var t in user.RefreshTokens.Where(t => t.IsActive))
        {
            t.Revoke();
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResult> GenerateTokensForUser(User user)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.AddRefreshToken(refreshToken);
        
        // Explicitly mark as Added to avoid concurrency issue where it is detected as Modified
        _context.Entry(refreshToken).State = EntityState.Added;
        
        await _context.SaveChangesAsync();

        return new AuthResult(accessToken, refreshToken.Token);
    }
}
