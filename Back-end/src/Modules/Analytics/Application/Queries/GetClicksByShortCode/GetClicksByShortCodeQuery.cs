using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetClicksByShortCode
{
    public sealed record GetClicksByShortCodeQuery(string ShortCode) : IRequest<IEnumerable<ClickEventResponse>>;
}
