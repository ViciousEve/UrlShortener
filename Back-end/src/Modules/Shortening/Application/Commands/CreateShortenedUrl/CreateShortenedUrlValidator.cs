using FluentValidation;
using Shortening.Domain;

namespace Shortening.Application.Commands.CreateShortenedUrl
{
    public class CreateShortenedUrlValidator : AbstractValidator<CreateShortenedUrlCommand>
    {
        private static readonly int[] AllowedTtlMinutes = ShortenedUrl.AllowedTtlInMinutes.ToArray();
        public CreateShortenedUrlValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Original URL is required.")
                .Must(BeAValidUrl).WithMessage("Original URL must be a valid URL.");

            RuleFor(x => x.TtlInMinutes)
                .Must(ttl => AllowedTtlMinutes.Contains(ttl))
                .WithMessage($"TTL in minutes must be one of the following values: {string.Join(", ", AllowedTtlMinutes)}.");

            //Ttl for anonymous user must be 15 minutes
            RuleFor( x => x.TtlInMinutes)
                .Equal(15).When(x => x.UserId == null)
                .WithMessage("Anonymous users can only create 15-minutes links");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}