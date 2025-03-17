using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URLShortener.DTOs;
using URLShortener.Services;

namespace URLShortener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly IValidator<ShortcodeUrlRequestDto> _urlShortCodeValidator;
        private readonly IValidator<ShortenedUrlDto> _originalUrlValidator;

        public UrlShortenerController(IUrlShortenerService urlShortenerService, IValidator<ShortcodeUrlRequestDto> urlShortCodeValidator, IValidator<ShortenedUrlDto> originalUrlValidator)
        {
            _urlShortenerService = urlShortenerService;
            _urlShortCodeValidator = urlShortCodeValidator;
            _originalUrlValidator = originalUrlValidator;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl(string originalUrl)
        {
           ValidationResult validationResult = _originalUrlValidator.ValidateAsync(new ShortenedUrlDto { OriginalUrl = originalUrl }).Result;
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _urlShortenerService.ShortenUrlAsync(originalUrl);
            return Ok(result);
        }

        [HttpGet("{shortcode}")]
        public async Task<IActionResult> GetOriginalUrl(string shortcode)
        {
            ValidationResult validationResult = _urlShortCodeValidator.ValidateAsync(new ShortcodeUrlRequestDto { ShortCode = shortcode }).Result;

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            try
            {
                var userAgent = Request.Headers.UserAgent.ToString();
                var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(shortcode, userAgent);
                return RedirectPermanentPreserveMethod(originalUrl);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("URL no encontrada o expirada");
            }
        }
    }
}
