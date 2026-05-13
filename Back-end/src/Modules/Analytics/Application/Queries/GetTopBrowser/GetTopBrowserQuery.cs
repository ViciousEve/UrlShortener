using MediatR;

namespace Analytics.Application.Queries.GetTopBrowser;

public record GetTopBrowserQuery(Guid UserId, int TopN = 5) : IRequest<IEnumerable<string>>;
