using Analytics.Application.DTOs;
using MediatR;

namespace Analytics.Application.Queries.GetTopUrl;

public record GetTopUrlQuery(Guid UserId) : IRequest<ShortenedUrlClickStats?>;
