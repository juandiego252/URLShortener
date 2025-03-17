using FluentValidation;
using URLShortener.DTOs;

namespace URLShortener.Validators
{
    public class UrlShortCodeValidator : AbstractValidator<ShortcodeUrlRequestDto>
    {
        public UrlShortCodeValidator()
        {
            RuleFor(x => x.ShortCode).Matches("^[a-zA-Z0-9]{6}$")
                .WithMessage("ShortCode must be 6 characters long and contain only letters and numbers.");
        }
    }
}
