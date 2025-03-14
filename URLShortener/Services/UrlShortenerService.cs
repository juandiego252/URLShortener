using System.Data;
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

            string cacheKey = $"shortcode:{shortcode}";
            var shortenedUrl = await _cacheService.GetAsync<ShortenedUrl>(cacheKey);
            DateTime nowDateTime = DateTime.UtcNow.ToLocalTime();

            if (shortenedUrl == null)
            {
                shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortcode);
                if (shortenedUrl == null || !shortenedUrl.IsActive)
                {
                    throw new KeyNotFoundException("URL not found or expired");
                }

                await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);
            }

            shortenedUrl.AccessCount++;
            shortenedUrl.LastAccessedAt = nowDateTime;

            await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);

            var urlAccess = new UrlAccess
            {
                ShortenedUrlId = shortenedUrl.Id,
                AccessedAt = nowDateTime,
                UserAgent = userAgent
            };
            await _urlRepository.AddUrlAccess(urlAccess);
            await _urlRepository.SaveChangesAsync();
            return shortenedUrl.OriginalUrl;
        }

        public async Task<ShortenedUrlDto> ShortenUrlAsync(string originalUrl)
        {
            string cacheKey = $"originalUrl:{originalUrl}";

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
                };

            }

            string shortCode = await GenerateUniqueShortCodeAsync();

            DateTime createdAt = DateTime.UtcNow.ToLocalTime();
            DateTime expiresAt = createdAt.Add(TimeSpan.FromHours(3));


            var newEntry = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = createdAt,
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
        public async Task<ShortenedUrlDto> RenewShortenUrlAsync(string shortcode, string userAgent)
        {

            string cacheKey = $"shortcode:{shortcode}";
            var shortenedUrl = await _cacheService.GetAsync<ShortenedUrl>(cacheKey);
            DateTime nowDateTime = DateTime.UtcNow.ToLocalTime();

            if (shortenedUrl == null)
            {
                shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortcode);
                if (shortenedUrl == null || !shortenedUrl.IsActive)
                {
                    throw new KeyNotFoundException("URL not found");
                }
                await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);
            }

            //if (shortenedUrl.ExpiresAt.HasValue && shortenedUrl.ExpiresAt.Value < nowDateTime)
            //{
            //    shortenedUrl.ExpiresAt = nowDateTime.AddHours(3);
            //}
            //else
            //{
            //    shortenedUrl.ExpiresAt = nowDateTime.AddHours(3);
            //}

            shortenedUrl.AccessCount++;
            shortenedUrl.LastAccessedAt = nowDateTime;

            await _urlRepository.SaveChangesAsync();
            await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);

            var urlAccess = new UrlAccess
            {
                ShortenedUrlId = shortenedUrl.Id,
                AccessedAt = nowDateTime,
                UserAgent = userAgent
            };
            await _urlRepository.AddUrlAccess(urlAccess);
            await _urlRepository.SaveChangesAsync();
            await _cacheService.SetAsync(cacheKey, shortenedUrl, _cacheExpiration);
            return new ShortenedUrlDto
            {
                OriginalUrl = shortenedUrl.OriginalUrl,
                ShortCode = shortenedUrl.ShortCode,
                ShortenedUrl = BaseUrl + shortenedUrl.ShortCode,
                CreatedAt = shortenedUrl.CreatedAt,
            };
        }
    }
}
