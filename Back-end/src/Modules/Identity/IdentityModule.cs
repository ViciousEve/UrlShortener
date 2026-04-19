using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Contracts;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;

namespace Identity
{
    /// <summary>
    /// Module registration for the Identity module.
    /// Same pattern as ShorteningModule — called from Program.cs.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This is the composition root for the Identity module.
    ///    It wires up all dependencies so the rest of the code uses DI.
    ///    
    /// 2. Registration order doesn't matter for DI, but logically:
    ///    a. DbContext first (needed by repositories)
    ///    b. Repositories (needed by handlers)
    ///    c. Services (JwtProvider, PasswordHasher — needed by handlers)
    ///    d. MediatR (discovers and registers handlers)
    ///    e. FluentValidation (discovers and registers validators)
    ///    
    /// 3. After creating this file, add to Program.cs:
    ///    - using Identity;
    ///    - using Identity.Presentation;
    ///    - builder.Services.AddIdentityModule(builder.Configuration);
    ///    - app.MapIdentityModule();
    ///    
    /// 4. Later, you'll also need to configure JWT authentication middleware:
    ///    - builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    ///        .AddJwtBearer(options => { ... });
    ///    - app.UseAuthentication();
    ///    - app.UseAuthorization();
    ///    This can be done in Program.cs or in a separate extension method here.
    /// </summary>
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
