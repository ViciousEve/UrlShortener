using App.Exceptions;
using MediatR;
using Shortening.Application.Contracts;

namespace Shortening.Application.Commands.DisableShortenedUrl
{
    public class DisableShortenedUrlCommandHandler : IRequestHandler<DisableShortenedUrlCommand>
    {
        private readonly IShortenedUrlRepository _repository;
        public DisableShortenedUrlCommandHandler(IShortenedUrlRepository repository)
        {
            _repository = repository;
        }
        public async Task Handle(DisableShortenedUrlCommand request, CancellationToken cancellationToken)
        {
            var shortenedUrl = await _repository.GetByShortCodeAsync(request.ShortCode);
            if (shortenedUrl == null)
                throw new NotFoundException("Url not found!");

            if(shortenedUrl.UserId != request.UserId)
                throw new ForbiddenAccessException();
            shortenedUrl.Disable();
            await _repository.SaveChangesAsync();
        }

    }
}