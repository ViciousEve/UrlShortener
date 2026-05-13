using Analytics.Application.Contracts;
using MediatR;

namespace Analytics.Application.Queries.GetTopBrowser;

public class GetTopBrowserHandler : IRequestHandler<GetTopBrowserQuery, IEnumerable<string>>
{
    private readonly IClickEventRepository _repository;

    public GetTopBrowserHandler(IClickEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<string>> Handle(GetTopBrowserQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTopBrowserForUserAsync(request.UserId, request.TopN);
    }
}
