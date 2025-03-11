using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URLShortener.Services;

namespace URLShortener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public UrlShortenerController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
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
            if (string.IsNullOrWhiteSpace(shortcode))
            {
                return BadRequest("El código corto no puede estar vacío");
            }

            try
            {
                var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(shortcode);
                return RedirectPermanentPreserveMethod(originalUrl);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("URL no encontrada o expirada");
            }
        }
    }
}
