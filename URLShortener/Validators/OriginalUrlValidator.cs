using System.Text.RegularExpressions;
using FluentValidation;
using URLShortener.DTOs;

namespace URLShortener.Validators
{
    public class OriginalUrlValidator : AbstractValidator<ShortenedUrlDto>
    {
        private const string BaseUrl = "https://localhost:7034/api/UrlShorten";
        public OriginalUrlValidator()
        {
            RuleFor(x => x.OriginalUrl).NotEmpty().WithMessage("La url no puede estar vaciá")
                .Must(BeAValidUrl).WithMessage("The URL is not valid")
                .Must(url => !url.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase))
                .WithMessage("The provided URL has already been shortened by this service");

        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                return false;

            if (!(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                return false;
            if (!IsValidDomain(uriResult.Host))
            {
                return false;
            }
            return true;
        }

        private bool IsValidDomain(string host)
        {
            // Permitir 'localhost' como dominio válido
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            string domainPattern = @"^((?=[a-z0-9-]{1,63}\.)(xn--)?[a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,63}$"; ;
            return Regex.IsMatch(host, domainPattern, RegexOptions.IgnoreCase);
        }
    }
}
