using Analytics.Application.Contracts;
using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetTopUrl;

public class GetTopUrlHandler : IRequestHandler<GetTopUrlQuery, ShortenedUrlClickStats?>
{
    private readonly IClickEventRepository _repository;

    public GetTopUrlHandler(IClickEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShortenedUrlClickStats?> Handle(GetTopUrlQuery request, CancellationToken cancellationToken)
    {
        var ranked = await _repository.GetTotalClickForUserRankedAsync(request.UserId);
        return ranked.FirstOrDefault();
    }
}
