using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using QueuePilot.Application.Auth.Commands;
using QueuePilot.Application.Tickets.Commands;
using QueuePilot.Application.Tickets.Queries;
using QueuePilot.Domain.Enums;
using Xunit;

namespace QueuePilot.IntegrationTests;

public class TicketTests : BaseIntegrationTest
{
    public TicketTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    private async Task<string> AuthenticateAsync(string email, UserRole role)
    {
        var registerCmd = new RegisterCommand(email, "Password123!", role);
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCmd);
        
        if (!response.IsSuccessStatusCode)
        {
             // Try login if already registered
             var loginCmd = new LoginCommand(email, "Password123!");
             response = await Client.PostAsJsonAsync("/api/auth/login", loginCmd);
        }
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return result!.AccessToken;
    }

    [Fact]
    public async Task CreateTicket_ShouldReturnTicketId_WhenAuthorized()
    {
        // Arrange
        var token = await AuthenticateAsync("customer1@queuepilot.com", UserRole.Customer);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = new { Title = "My Support Ticket", Description = "Help needed" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tickets", dto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TicketResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    public record TicketResponse(Guid Id);

    [Fact]
    public async Task GetTickets_ShouldReturnList_WhenAuthorized()
    {
        // Arrange
        var token = await AuthenticateAsync("customer2@queuepilot.com", UserRole.Customer);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = new { Title = "Ticket A", Description = "Desc A" };
        await Client.PostAsJsonAsync("/api/tickets", dto);

        // Act
        var response = await Client.GetAsync("/api/tickets");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TicketDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }
}
