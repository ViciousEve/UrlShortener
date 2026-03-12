using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
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
                throw new KeyNotFoundException($"Shortened URL not found for code: {request.ShortCode}");
            }

            if(shortenedUrl.Status == Domain.UrlStatus.Expired)
            {
                throw new Exception("Shortened URL has expired");
            }
            // Ttl check in case background job hasn't run yet to update status to expired
            if (shortenedUrl.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Shortened URL has expired");
            }

            if(shortenedUrl.Status == Domain.UrlStatus.Disabled)
            {
                throw new Exception("Shortened URL is disabled");
            }

            //Publish an event for analytics
            var clickedEvent = new UrlClickedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                shortenedUrl.Id,
                shortenedUrl.ShortCode.Value,
                shortenedUrl.OriginalUrl,
                shortenedUrl.UserId
            );

            await _publisher.Publish(clickedEvent, cancellationToken);

            return shortenedUrl.OriginalUrl;
        }

    }
}
