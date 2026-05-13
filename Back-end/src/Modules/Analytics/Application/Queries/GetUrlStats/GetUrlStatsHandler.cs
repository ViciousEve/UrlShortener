using Analytics.Application.Contracts;
using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetUrlStats
{
    public sealed class GetUrlStatsHandler : IRequestHandler<GetUrlStatsQuery, UrlStatsResponse?>
    {
        private readonly IClickEventRepository _repository;

        public GetUrlStatsHandler(IClickEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<UrlStatsResponse?> Handle(GetUrlStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _repository.GetStatsByShortCodeAsync(request.ShortCode);

            if (stats == null)
            {
                return null;
            }

            return new UrlStatsResponse(
                stats.Id,
                stats.ShortCode,
                stats.TotalClicks,
                stats.LastClickedAtUtc
            );
        }
    }
}