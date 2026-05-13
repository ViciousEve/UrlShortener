using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Contracts;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;

namespace Identity
{
    public static class IdentityModule
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext — uses the same connection string as other modules
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register Application layer contracts → Infrastructure implementations
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            // MediatR — discovers all handlers in this assembly
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(IdentityModule).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // FluentValidation — discovers all validators in this assembly
            services.AddValidatorsFromAssembly(typeof(IdentityModule).Assembly);

            return services;
        }
    }
}
