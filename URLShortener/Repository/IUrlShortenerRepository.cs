using URLShortener.Models;

namespace URLShortener.Repository
{
    public interface IUrlShortenerRepository
    {
        Task<ShortenedUrl> GetByShortCodeAsync(string shortCode);
        Task<ShortenedUrl> GetByOriginalUrlCodeAsync(string orignalUrl);
        Task AddAsync(ShortenedUrl url);
        Task AddUrlAccess(UrlAccess urlAccess);
        ShortenedUrl IncrementAccessCount(ShortenedUrl shortenedUrl, UrlAccess urlAccess);
        Task SaveChangesAsync();
    }
}
