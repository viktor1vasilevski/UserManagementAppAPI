namespace Infrastructure.IoC;

using Main.Interfaces;
using Main.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddIoCService(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();


        return services;
    }
}
