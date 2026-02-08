using Microsoft.Extensions.DependencyInjection;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Infrastructure.Services;

namespace QueuePilot.Infrastructure;

public static partial class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
