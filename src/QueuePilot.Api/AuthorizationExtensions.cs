using QueuePilot.Domain.Enums;

namespace QueuePilot.Api;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole(UserRole.Admin.ToString()));
                
            options.AddPolicy("AgentOnly", policy => 
                policy.RequireRole(UserRole.Agent.ToString(), UserRole.Admin.ToString()));
        });
            
        return services;
    }
}
