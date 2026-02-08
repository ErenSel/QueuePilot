using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QueuePilot.Infrastructure.Persistence;
using Xunit;

namespace QueuePilot.IntegrationTests;

[Collection("IntegrationTests")] // Share context if needed, or use IClassFixture
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly AppDbContext DbContext;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Client = factory.CreateClient();
    }
}
