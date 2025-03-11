using Microsoft.EntityFrameworkCore;
using URLShortener.Models;

namespace URLShortener.Infrastructure.Database
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        { }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        public DbSet<UrlAccess> UrlAccesses { get; set; }
    }
}
