using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetUrlStats
{
    public sealed record GetUrlStatsQuery(string ShortCode) : IRequest<UrlStatsResponse?>;
}