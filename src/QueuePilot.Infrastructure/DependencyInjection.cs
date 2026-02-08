using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Infrastructure.Authentication;
using QueuePilot.Infrastructure.Persistence;
using QueuePilot.Infrastructure.Services;

namespace QueuePilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        services.Configure<Messaging.RabbitMQOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IEventBus, Messaging.RabbitMQEventBus>();
        services.AddHostedService<Messaging.EventConsumerService>();

        return services;
    }
}
