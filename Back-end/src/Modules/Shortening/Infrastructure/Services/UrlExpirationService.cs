using Shortening.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace Shortening.Infrastructure.Services
{
    public class UrlExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UrlExpirationService> _logger;

        public UrlExpirationService(IServiceProvider serviceProvider, ILogger<UrlExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UrlExpirationService started!");
            while(!stoppingToken.IsCancellationRequested)
            {
                using(var scope = _serviceProvider.CreateScope())
                {
                    var _repository = scope.ServiceProvider.GetRequiredService<IShortenedUrlRepository>();

                    var expiredUrls = await _repository.GetExpiredUrlsAsync();
                    foreach (var url in expiredUrls)
                    {
                        url.Expire();
                    }
                    await _repository.SaveChangesAsync();
                }
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
