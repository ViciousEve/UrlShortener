using MediatR;

namespace Analytics.Application.Queries.GetUserClicksInPeriod;

public record GetUserClicksInPeriodQuery(Guid UserId, DateTime FromDate, DateTime ToDate) : IRequest<int>;
