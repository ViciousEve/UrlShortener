using MediatR;
using Microsoft.Extensions.Options;
using Shortening.Application.Configuration;
using Shortening.Application.Contracts;
using Shortening.Application.DTOs;
using Shortening.Domain;

namespace Shortening.Application.Commands.CreateShortenedUrl
{
    public class CreateShortenedUrlHandler : IRequestHandler<CreateShortenedUrlCommand, ShortenedUrlResponse>
    {
        private readonly IShortenedUrlRepository _repository;
        private readonly IShortCodeGenerator _shortCodeGenerator;
        private readonly AppUrlSettings _appUrlSettings;

        public CreateShortenedUrlHandler(IShortenedUrlRepository repository, 
            IShortCodeGenerator shortCodeGenerator,
            IOptions<AppUrlSettings> appUrlSettings)
        {
            _repository = repository;
            _shortCodeGenerator = shortCodeGenerator;
            _appUrlSettings = appUrlSettings.Value;
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
                ShortUrl = $"{_appUrlSettings.AppUrl}/s/{shortenedUrl.ShortCode.Value}",
                OriginalUrl = shortenedUrl.OriginalUrl,
                CreatedAt = shortenedUrl.CreatedAt,
                ExpiresAt = shortenedUrl.ExpiresAt,
                Status = shortenedUrl.Status.ToString()
            };
        }
    }
}