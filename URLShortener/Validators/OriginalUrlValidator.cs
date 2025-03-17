using System.Text.RegularExpressions;
using FluentValidation;
using URLShortener.DTOs;

namespace URLShortener.Validators
{
    public class OriginalUrlValidator : AbstractValidator<ShortenedUrlDto>
    {

        public OriginalUrlValidator()
        {
            RuleFor(x => x.OriginalUrl).NotEmpty().WithMessage("La url no puede estar vaciá")
                .Must(BeAValidUrl).WithMessage("La url no es válida");

        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                return false;

            return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }
    }
}
