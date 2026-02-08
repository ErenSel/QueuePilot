using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueuePilot.Application.Auth.Commands;
using QueuePilot.Application.Common.Interfaces;

namespace QueuePilot.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await _authService.RegisterAsync(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _authService.LoginAsync(command);
        return Ok(result);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshStepCommand command)
    {
         var result = await _authService.RefreshTokenAsync(command);
         return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        await _authService.RevokeCurrentUserAsync(email);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RevokeCommand command)
    {
        await _authService.RevokeTokenAsync(command);
        return NoContent();
    }
}
