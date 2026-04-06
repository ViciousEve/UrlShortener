using Analytics.Application.Contracts;
using MediatR;

namespace Analytics.Application.Queries.GetUserClicksInPeriod;

public class GetUserClicksInPeriodHandler : IRequestHandler<GetUserClicksInPeriodQuery, int>
{
    private readonly IClickEventRepository _repository;

    public GetUserClicksInPeriodHandler(IClickEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetUserClicksInPeriodQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTotalClickForUserInPeriodAsync(request.UserId, request.FromDate, request.ToDate);
    }
}
