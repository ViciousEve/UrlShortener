using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
            //Register application services
            services.AddScoped<IShortenedUrlRepository, ShortenedUrlRepository>();
            services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
            //MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ShorteningModule).Assembly));
            //FluentValidation
            services.AddValidatorsFromAssembly(typeof(ShorteningModule).Assembly);

            return services;
        }
    }
}
