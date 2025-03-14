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
        private readonly IValidator<ShortenedUrlDto> _urlShortCodeValidator;

        public UrlShortenerController(IUrlShortenerService urlShortenerService, IValidator<ShortenedUrlDto> urlShortCodeValidator)
        {
            _urlShortenerService = urlShortenerService;
            _urlShortCodeValidator = urlShortCodeValidator;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                return BadRequest("La url no puede estar vaciá");
            }

            var result = await _urlShortenerService.ShortenUrlAsync(originalUrl);
            return Ok(result);
        }

        [HttpGet("{shortcode}")]
        public async Task<IActionResult> GetOriginalUrl(string shortcode)
        {
            ValidationResult validationResult = _urlShortCodeValidator.ValidateAsync(new ShortenedUrlDto { ShortCode = shortcode }).Result;

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
