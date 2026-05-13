using Analytics.Application.Contracts;
using Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics
{
    public static class AnalyticsModule
    {
        public static IServiceCollection AddAnalyticsModule(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AnalyticsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register application services
            services.AddScoped<IClickEventRepository, ClickEventRepository>();

            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(AnalyticsModule).Assembly));

            return services;
        }
    }
}
