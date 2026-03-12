using Shortening.Application.Contracts;
using Microsoft.Extensions.Hosting;


namespace Shortening.Infrastructure.Services
{
    public class UrlExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public UrlExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}