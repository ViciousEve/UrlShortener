using MediatR;
using Shortening.Application.Contracts;
using Shortening.Application.DTOs;
using Shortening.Domain;

namespace Shortening.Application.Commands.CreateShortenedUrl
{
    public class CreateShortenedUrlHandler : IRequestHandler<CreateShortenedUrlCommand, ShortenedUrlResponse>
    {
        private readonly IShortenedUrlRepository _repository;
        private readonly IShortCodeGenerator _shortCodeGenerator;

        public CreateShortenedUrlHandler(IShortenedUrlRepository repository, 
            IShortCodeGenerator shortCodeGenerator)
        {
            _repository = repository;
            _shortCodeGenerator = shortCodeGenerator;
        }


        public async Task<ShortenedUrlResponse> Handle(CreateShortenedUrlCommand request, CancellationToken cancellationToken)
        {
            //generate short code
            ShortCode shortCode = new ShortCode(_shortCodeGenerator.GenerateShortCode());

            //Create shortened URL entity
            ShortenedUrl shortenedUrl = new ShortenedUrl(
                shortCode: shortCode, 
                originalUrl: request.OriginalUrl,
                expiresAt: DateTime.UtcNow.AddMinutes(request.TtlInMinutes),
                userId: request.UserId);

            //persist the entity
            await _repository.AddAsync(shortenedUrl);
            await _repository.SaveChangesAsync();

            //return the response
            return new ShortenedUrlResponse
            {
                ShortCode = shortenedUrl.ShortCode.Value,
                OriginalUrl = shortenedUrl.OriginalUrl,
                CreatedAt = shortenedUrl.CreatedAt,
                ExpiresAt = shortenedUrl.ExpiresAt,
                Status = shortenedUrl.Status.ToString()
            };
        }
    }
}