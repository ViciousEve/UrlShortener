using MediatR;
using App.Exceptions;
using Shortening.Application.Contracts;
using Shortening.Application.IntegrationEvents;

namespace Shortening.Application.Queries.ResolveShortCode
{
    public class ResolveShortCodeHandler : IRequestHandler<ResolveShortCodeQuery, string>
    {
        private readonly IShortenedUrlRepository _repository;
        private readonly IPublisher _publisher;

        public ResolveShortCodeHandler(IShortenedUrlRepository repository, IPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }
        public async Task<string> Handle(ResolveShortCodeQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the shortened URL entity from the repository
            var shortenedUrl = await _repository.GetByShortCodeAsync(request.ShortCode);
            if (shortenedUrl == null)
            {
                throw new NotFoundException("ShortenedUrl", request.ShortCode);
            }

            if (shortenedUrl.Status == Domain.UrlStatus.Expired)
            {
                throw new ExpiredUrlException(request.ShortCode);
            }
            // Ttl check in case background job hasn't run yet to update status to expired
            if (shortenedUrl.ExpiresAt < DateTime.UtcNow)
            {
                throw new ExpiredUrlException(request.ShortCode);
            }

            if (shortenedUrl.Status == Domain.UrlStatus.Disabled)
            {
                throw new NotFoundException("ShortenedUrl", request.ShortCode);
            }

            //Publish an event for analytics if user is not anonymous
            if (shortenedUrl.UserId != null)
            {
                var clickedEvent = new UrlClickedIntegrationEvent(
                    shortenedUrl.Id,
                    shortenedUrl.ShortCode.Value,
                    shortenedUrl.OriginalUrl,
                    shortenedUrl.UserId,
                    request.IpAddress,
                    request.UserAgent
                );

                await _publisher.Publish(clickedEvent, cancellationToken);
            }

            return shortenedUrl.OriginalUrl;
        }

    }
}
