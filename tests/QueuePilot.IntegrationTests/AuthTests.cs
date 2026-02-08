using System.Net.Http.Json;
using FluentAssertions;
using QueuePilot.Application.Auth.Commands;
using QueuePilot.Domain.Enums;
using Xunit;

namespace QueuePilot.IntegrationTests;

public class AuthTests : BaseIntegrationTest
{
    public AuthTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldReturnTokens_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterCommand("test@queuepilot.com", "Password123!", UserRole.Customer);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var registerCmd = new RegisterCommand("loginuser@queuepilot.com", "Password123!", UserRole.Agent);
        await Client.PostAsJsonAsync("/api/auth/register", registerCmd);

        var loginCmd = new LoginCommand("loginuser@queuepilot.com", "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginCmd);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        result.Should().NotBeNull();
    }
}
