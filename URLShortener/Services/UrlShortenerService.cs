using System.Text;
using Microsoft.EntityFrameworkCore;
using URLShortener.DTOs;
using URLShortener.Infrastructure.Database;
using URLShortener.Models;
using URLShortener.Repository;

namespace URLShortener.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly IUrlShortenerRepository _urlRepository;

        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string BaseUrl = "https://localhost:7034/";

        public UrlShortenerService(IUrlShortenerRepository urlRepository)
        {
            _urlRepository = urlRepository;
        }

        public async Task<string> GetOriginalUrlAsync(string shortcode, string userAgent)
        {
            var shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortcode);
            if (shortenedUrl == null || !shortenedUrl.IsActive || (shortenedUrl.ExpiresAt.HasValue && shortenedUrl.ExpiresAt.Value < DateTime.UtcNow))
            {
                throw new KeyNotFoundException("URL not found or expired.");
            }

            shortenedUrl.AccessCount++;
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            shortenedUrl.LastAccessedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            var urlAccess = new UrlAccess
            {
                ShortenedUrlId = shortenedUrl.Id,
                AccessedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo),
                UserAgent = userAgent
            };
            await _urlRepository.AddUrlAccess(urlAccess);
            await _urlRepository.SaveChangesAsync();
            return shortenedUrl.OriginalUrl;
        }

        public async Task<ShortenedUrlDto> ShortenUrlAsync(string originalUrl)
        {

            TimeSpan timeSpan = TimeSpan.FromHours(3);

            // Verificar si existe la url en la base de datos
            var existUrl = await _urlRepository.GetByOriginalUrlCodeAsync(originalUrl);

            if (existUrl != null)
            {
                return new ShortenedUrlDto
                {
                    OriginalUrl = existUrl.OriginalUrl,
                    ShortCode = existUrl.ShortCode,
                    ShortenedUrl = BaseUrl + existUrl.ShortCode,
                    CreatedAt = existUrl.CreatedAt,
                    ExpiresAt = existUrl.ExpiresAt
                };

            }

            string shortCode = await GenerateUniqueShortCodeAsync();

            var newEntry = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(timeSpan),
                AccessLogs = new List<UrlAccess>()
            };

            await _urlRepository.AddAsync(newEntry);
            await _urlRepository.SaveChangesAsync();

            return new ShortenedUrlDto
            {
                OriginalUrl = newEntry.OriginalUrl,
                ShortCode = newEntry.ShortCode,
                ShortenedUrl = BaseUrl + newEntry.ShortCode,
                CreatedAt = newEntry.CreatedAt,
                ExpiresAt = newEntry.ExpiresAt,
            };
        }

        // Método para generar un código corto único aleatorio
        private async Task<string> GenerateUniqueShortCodeAsync(int length = 6)
        {
            bool isUnique = false;
            string code = "";
            Random random = new Random();

            while (!isUnique)
            {
                StringBuilder sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {
                    sb.Append(Characters[random.Next(Characters.Length)]);
                }

                code = sb.ToString();

                // Verificar que el código no exista ya en la base de datos
                var existingUrl = await _urlRepository.GetByShortCodeAsync(code);
                if (existingUrl == null)
                {
                    isUnique = true;
                }
            }

            return code;
        }
    }
}
