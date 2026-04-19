using Analytics.Application.Contracts;
using Analytics.Domain;
using MediatR;
using Shortening.Application.IntegrationEvents;

namespace Analytics.Application.IntegrationEventHandlers
{
    public class UrlClickedIntegrationEventHandler : INotificationHandler<UrlClickedIntegrationEvent>
    {
        private readonly IClickEventRepository _repository;

        public UrlClickedIntegrationEventHandler(IClickEventRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(UrlClickedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            // 1. Upsert ShortenedUrlStats
            var stats = await _repository.GetStatsByIdAsync(notification.ShortenedUrlId);
            if (stats == null)
            {
                stats = new ShortenedUrlStats(
                    notification.ShortenedUrlId,
                    notification.ShortCode,
                    notification.OriginalUrl,
                    notification.UserId
                );
                await _repository.AddStatsAsync(stats);
            }

            // 2. Update aggregated stats
            stats.RecordClick();

            // 3. Add ClickEvent with metadata
            var clickEvent = new ClickEvent(
                stats.Id,
                notification.OccurredOnUtc,
                notification.IpAddress,
                notification.UserAgent
            );

            await _repository.AddClickAsync(clickEvent);
            
            // 4. Save everything
            await _repository.SaveChangesAsync();
        }
    }
}
