using URLShortener.DTOs;

namespace URLShortener.Services
{
    public interface IUrlShortenerService
    {
        Task<ShortenedUrlDto> ShortenUrlAsync(string originalUrl);
        Task<string> GetOriginalUrlAsync(string shortcode);
    }
}
