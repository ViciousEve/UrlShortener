using MediatR;

namespace Shortening.Application.Queries.ResolveShortCode
{  
    public record ResolveShortCodeQuery(string ShortCode, string? IpAddress = null, string? UserAgent = null) : IRequest<string>;
}
