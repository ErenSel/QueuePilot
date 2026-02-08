using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QueuePilot.Application.Auth.Commands;
using QueuePilot.Application.Common.Exceptions;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Common;
using QueuePilot.Domain.Enums;
using Xunit;

namespace QueuePilot.IntegrationTests;

public class MessagingFailureTests : IClassFixture<MessagingFailureTests.MessagingFailureWebAppFactory>
{
    private readonly HttpClient _client;

    public MessagingFailureTests(MessagingFailureWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicket_ShouldReturnServiceUnavailable_WhenPublishFails()
    {
        var token = await AuthenticateAsync("customer-failure@queuepilot.com", UserRole.Customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = new { Title = "RabbitMQ down", Description = "Expect 503" };

        var response = await _client.PostAsJsonAsync("/api/tickets", dto);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private async Task<string> AuthenticateAsync(string email, UserRole role)
    {
        var registerCmd = new RegisterCommand(email, "Password123!", role);
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerCmd);

        if (!response.IsSuccessStatusCode)
        {
            var loginCmd = new LoginCommand(email, "Password123!");
            response = await _client.PostAsJsonAsync("/api/auth/login", loginCmd);
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return result!.AccessToken;
    }

    public class MessagingFailureWebAppFactory : IntegrationTestWebAppFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEventBus>();
                services.AddSingleton<IEventBus, FailingEventBus>();
            });
        }
    }

    private sealed class FailingEventBus : IEventBus
    {
        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new MessagingUnavailableException("RabbitMQ channel unavailable for testing.");
        }
    }
}
