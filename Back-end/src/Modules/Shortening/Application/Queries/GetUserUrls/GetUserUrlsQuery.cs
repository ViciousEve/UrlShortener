using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shortening.Application.DTOs;

namespace Shortening.Application.Queries.GetUserUrls
{
    public record GetUserUrlsQuery(Guid UserId): IRequest<IEnumerable<ShortenedUrlResponse>>;
}
