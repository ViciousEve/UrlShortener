using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Shortening.Domain.Events;

namespace Shortening.Application.DomainEventsHandler
{
    public sealed class UrlExpiredDomainEventHandler
        : INotificationHandler<UrlExpiredDomainEvent>
    {
        private readonly IMemoryCache _cache;
        public UrlExpiredDomainEventHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task Handle(UrlExpiredDomainEvent notification, CancellationToken cancellationToken)
        {
            var cacheKey = $"shortcode:{notification.ShortCode}";
            if(cacheKey != null)
            {
                _cache.Remove(cacheKey);
            }

            return Task.CompletedTask;
        }
    }
}
