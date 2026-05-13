using Analytics.Application.Contracts;
using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetClicksByShortCode
{
    public class GetClicksByShortCodeHandler : IRequestHandler<GetClicksByShortCodeQuery, IEnumerable<ClickEventResponse>>
    {
        private readonly IClickEventRepository _repository;

        public GetClicksByShortCodeHandler(IClickEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ClickEventResponse>> Handle(GetClicksByShortCodeQuery request, CancellationToken cancellationToken)
        {
            var clickEvents = await _repository.GetClicksByShortCodeAsync(request.ShortCode);

            return clickEvents.Select(c => new ClickEventResponse(
                c.Id,
                c.ShortenedUrlStatsId,
                c.ShortenedUrlStats.ShortCode,
                c.ShortenedUrlStats.OriginalUrl,
                c.ShortenedUrlStats.UserId,
                c.ClickedAtUtc
            ));
        }
    }
}
