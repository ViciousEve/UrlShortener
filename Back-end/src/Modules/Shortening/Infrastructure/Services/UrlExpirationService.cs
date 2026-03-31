using Shortening.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Shortening.Infrastructure.Services
{
    public class UrlExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public UrlExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
