using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shortening.Application.Contracts;
using Shortening.Application.DTOs;

namespace Shortening.Application.Queries.GetUserUrls
{
    public class GetUserUrlsHandler : IRequestHandler<GetUserUrlsQuery, IEnumerable<ShortenedUrlResponse>>
    {
        private readonly IShortenedUrlRepository _repository;

        public GetUserUrlsHandler(IShortenedUrlRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<ShortenedUrlResponse>> Handle(GetUserUrlsQuery request, CancellationToken cancellationToken)
        {
            var shortenedUrls = await _repository.GetByUserIdAsync(request.UserId);
            if(shortenedUrls == null || !shortenedUrls.Any())
            {
                return Enumerable.Empty<ShortenedUrlResponse>();
            }
            
            return shortenedUrls.Select(s => new ShortenedUrlResponse
            {
                ShortCode = s.ShortCode.Value,
                OriginalUrl = s.OriginalUrl,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                Status = s.Status.ToString()
            });
        }
    }
}
