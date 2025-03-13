using System.Text;
using Microsoft.EntityFrameworkCore;
using URLShortener.DTOs;
using URLShortener.Infrastructure.cache;
using URLShortener.Infrastructure.Database;
using URLShortener.Models;
using URLShortener.Repository;

namespace URLShortener.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly IUrlShortenerRepository _urlRepository;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string BaseUrl = "https://localhost:7034/api/UrlShortener/";

        public UrlShortenerService(IUrlShortenerRepository urlRepository, ICacheService cacheService)
        {
            _urlRepository = urlRepository;
            _cacheService = cacheService;
        }

        public async Task<string> GetOriginalUrlAsync(string shortcode, string userAgent)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            string cacheKey = $"shortcode:{shortcode}";
            var shortenedUrl = await _cacheService.GetAsync<ShortenedUrl>(cacheKey);
            DateTime nowDateTime = DateTime.UtcNow.ToLocalTime();

            if (shortenedUrl == null)
            {
                shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortcode);
                if (shortenedUrl == null || !shortenedUrl.IsActive || (shortenedUrl.ExpiresAt.HasValue && shortenedUrl.ExpiresAt.Value < nowDateTime))
                {
                    throw new KeyNotFoundException("URL not found or expired");
                }

                await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);
            }

            shortenedUrl.AccessCount++;
            shortenedUrl.LastAccessedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);

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
            string cacheKey = $"originalUrl:{originalUrl}";
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Verificar si existe la url en la cache
            var existUrl = await _cacheService.GetAsync<ShortenedUrl>(cacheKey);

            if (existUrl == null)
            {
                // Obtiene la url original de la base de datos
                existUrl = await _urlRepository.GetByOriginalUrlCodeAsync(originalUrl);
                if (existUrl != null)
                {
                    // Si la url existe la guarda en la cache
                    await _cacheService.SetAsync(cacheKey, existUrl, _cacheExpiration);
                }
            }

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

            DateTime createdAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
            DateTime expiresAt = createdAt.Add(TimeSpan.FromHours(3));


            var newEntry = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
                AccessLogs = new List<UrlAccess>()
            };

            await _urlRepository.AddAsync(newEntry);
            await _urlRepository.SaveChangesAsync();

            // Cache para la nueva url acortada
            await _cacheService.SetAsync(cacheKey, newEntry, _cacheExpiration);

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
            Random random = new();

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
