using Microsoft.EntityFrameworkCore;
using URLShortener.Infrastructure.Database;
using URLShortener.Models;

namespace URLShortener.Repository
{
    public class UrlShortenerRepository : IUrlShortenerRepository
    {
        private StoreContext _context;

        public UrlShortenerRepository(StoreContext context)
        {
            _context = context;
        }

        public async Task<ShortenedUrl> GetByShortCodeAsync(string shortCode)
        {
            return await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }
        public async Task<ShortenedUrl> GetByOriginalUrlCodeAsync(string orignalUrl)
        {
            return await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.OriginalUrl == orignalUrl);
        }
        public async Task AddAsync(ShortenedUrl url)
        {
            await _context.ShortenedUrls.AddAsync(url);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
