using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shortening.Application.Contracts;
using Shortening.Infrastructure.Persistence;
using Shortening.Infrastructure.Services;

namespace Shortening
{
    public static class ShorteningModule
    {
        public static IServiceCollection AddShorteningModule(this IServiceCollection services, IConfiguration configuration)
        {
            //DbContext
            services.AddDbContext<ShorteningDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            //Caching
            services.AddMemoryCache();

            //Register application services
            services.AddScoped<ShortenedUrlRepository>();
            services.AddScoped<IShortenedUrlRepository, CachedShortenedUrlRepository>();
            services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();

            //MediatR
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(typeof(ShorteningModule).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                });

            //FluentValidation
            services.AddValidatorsFromAssembly(typeof(ShorteningModule).Assembly);

            //Register background service
            services.AddHostedService<UrlExpirationService>();

            return services;
        }
    }
}
