using MediatR;
using Shortening.Application.Contracts;

namespace Shortening.Application.Commands.DeleteShortenedUrl
{
    public class DeleteShortenedUrlHandler : IRequestHandler<DeleteShortenedUrlCommand>
    {
        private readonly IShortenedUrlRepository _repository;
        public DeleteShortenedUrlHandler(IShortenedUrlRepository repository)
        {
            _repository = repository;
        }
        public async Task Handle(DeleteShortenedUrlCommand request, CancellationToken cancellationToken)
        {
            // Check if the shortened URL exists
            var shortenedUrl = await _repository.GetByShortCodeAsync(request.ShortCode);
            if (shortenedUrl == null)
            {
                    throw new Exception("Short code not found");
            }
            //Verify ownership
            if (shortenedUrl.UserId != request.UserId || request.UserId == null)
            {
                throw new Exception("Unauthorized to delete this URL");
            }
            await _repository.DeleteAsync(request.ShortCode);
            await _repository.SaveChangesAsync();
        }


    }
}