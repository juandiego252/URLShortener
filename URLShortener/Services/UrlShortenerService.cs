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
        private readonly string _baseUrl;

        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public UrlShortenerService(IUrlShortenerRepository urlRepository, ICacheService cacheService, string baseUrl)
        {
            _urlRepository = urlRepository;
            _cacheService = cacheService;
            _baseUrl = baseUrl;
        }

        public async Task<string> GetOriginalUrlAsync(string shortcode, string userAgent)
        {

            string cacheKey = $"shortcode:{shortcode}";
            var cachedUrl = await _cacheService.GetAsync<ShortenedUrlCacheDto>(cacheKey);
            DateTime nowDateTime = DateTime.UtcNow.ToLocalTime();

            ShortenedUrl shortenedUrl;

            if (cachedUrl == null)
            {
                shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortcode);
                if (shortenedUrl == null || !shortenedUrl.IsActive)
                {
                    throw new KeyNotFoundException("URL not found or Inactive");
                }

                var cacheDto = new ShortenedUrlCacheDto
                {
                    Id = shortenedUrl.Id,
                    OriginalUrl = shortenedUrl.OriginalUrl,
                    ShortCode = shortenedUrl.ShortCode,
                    CreatedAt = shortenedUrl.CreatedAt,
                    IsActive = shortenedUrl.IsActive,
                    AccessCount = shortenedUrl.AccessCount,
                    LastAccessedAt = shortenedUrl.LastAccessedAt
                };

                await _cacheService.SetAsync(cacheKey, cacheDto, _cacheExpiration);
            }
            else
            {
                shortenedUrl = new ShortenedUrl
                {
                    Id = cachedUrl.Id,
                    OriginalUrl = cachedUrl.OriginalUrl,
                    ShortCode = cachedUrl.ShortCode,
                    CreatedAt = cachedUrl.CreatedAt,
                    IsActive = cachedUrl.IsActive,
                    AccessCount = cachedUrl.AccessCount,
                    LastAccessedAt = cachedUrl.LastAccessedAt
                };
            }

            var urlAccess = new UrlAccess
            {
                ShortenedUrlId = shortenedUrl.Id,
                AccessedAt = nowDateTime,
                UserAgent = userAgent
            };

            shortenedUrl = _urlRepository.IncrementAccessCount(shortenedUrl, urlAccess);
            await _urlRepository.SaveChangesAsync();

            var UpdatedCacheDto = new ShortenedUrlCacheDto
            {
                Id = shortenedUrl.Id,
                OriginalUrl = shortenedUrl.OriginalUrl,
                ShortCode = shortenedUrl.ShortCode,
                CreatedAt = shortenedUrl.CreatedAt,
                IsActive = shortenedUrl.IsActive,
                AccessCount = shortenedUrl.AccessCount,
                LastAccessedAt = shortenedUrl.LastAccessedAt
            };
            await _cacheService.SetAsync(cacheKey, UpdatedCacheDto, _cacheExpiration);
            string originalUrlCacheKey = $"originalUrl:{shortenedUrl.OriginalUrl}";
            await _cacheService.SetAsync(originalUrlCacheKey, shortenedUrl, _cacheExpiration);
            return shortenedUrl.OriginalUrl;
        }

        public async Task<ShortenedUrlDto> ShortenUrlAsync(string originalUrl)
        {
            string cacheKey = $"originalUrl:{originalUrl}";

            if (originalUrl.StartsWith(_baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La URL proporcionada ya ha sido acortada por este servicio.");
            }

            // Verificar si existe la url en la cache
            var cacheUrl = await _cacheService.GetAsync<ShortenedUrlCacheDto>(cacheKey);
            ShortenedUrl existUrl = null;

            if (cacheUrl == null)
            {
                // Obtiene la url original de la base de datos
                existUrl = await _urlRepository.GetByOriginalUrlCodeAsync(originalUrl);
                if (existUrl != null)
                {
                    var cacheDto = new ShortenedUrlCacheDto
                    {
                        Id = existUrl.Id,
                        OriginalUrl = existUrl.OriginalUrl,
                        ShortCode = existUrl.ShortCode,
                        CreatedAt = existUrl.CreatedAt,
                        IsActive = existUrl.IsActive,
                        AccessCount = existUrl.AccessCount,
                        LastAccessedAt = existUrl.LastAccessedAt
                    };
                    // Si la url existe la guarda en la cache
                    await _cacheService.SetAsync(cacheKey, cacheDto, _cacheExpiration);
                    string shortCodeCacheKey = $"shortcode:{existUrl.ShortCode}";
                    await _cacheService.SetAsync(shortCodeCacheKey, cacheDto, _cacheExpiration);
                }
            }
            else
            {
                existUrl = new ShortenedUrl
                {
                    Id = cacheUrl.Id,
                    OriginalUrl = cacheUrl.OriginalUrl,
                    ShortCode = cacheUrl.ShortCode,
                    CreatedAt = cacheUrl.CreatedAt,
                    IsActive = cacheUrl.IsActive,
                    AccessCount = cacheUrl.AccessCount,
                    LastAccessedAt = cacheUrl.LastAccessedAt
                };
            }

            if (existUrl != null)
            {
                return new ShortenedUrlDto
                {
                    OriginalUrl = existUrl.OriginalUrl,
                    ShortCode = existUrl.ShortCode,
                    ShortenedUrl = _baseUrl + existUrl.ShortCode,
                    AccessCount = existUrl.AccessCount,
                    CreatedAt = existUrl.CreatedAt,
                };

            }

            string shortCode = await GenerateUniqueShortCodeAsync();
            DateTime createdAt = DateTime.UtcNow.ToLocalTime();


            var newEntry = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = createdAt,
                AccessCount = 0,
                IsActive = true,
                LastAccessedAt = null,
                AccessLogs = new List<UrlAccess>()
            };

            await _urlRepository.AddAsync(newEntry);
            await _urlRepository.SaveChangesAsync();

            var newCacheDto = new ShortenedUrlCacheDto
            {
                Id = newEntry.Id,
                OriginalUrl = newEntry.OriginalUrl,
                ShortCode = newEntry.ShortCode,
                CreatedAt = newEntry.CreatedAt,
                IsActive = newEntry.IsActive,
                AccessCount = newEntry.AccessCount,
                LastAccessedAt = newEntry.LastAccessedAt
            };

            // Cache para la nueva url acortada
            await _cacheService.SetAsync(cacheKey, newCacheDto, _cacheExpiration);
            string newShortCodeCacheKey = $"shortcode:{shortCode}";
            await _cacheService.SetAsync(newShortCodeCacheKey, newCacheDto, _cacheExpiration);

            return new ShortenedUrlDto
            {
                OriginalUrl = newEntry.OriginalUrl,
                ShortCode = newEntry.ShortCode,
                ShortenedUrl = _baseUrl + newEntry.ShortCode,
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
    }
}
