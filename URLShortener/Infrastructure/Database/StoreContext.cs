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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>()
                .HasMany(su => su.AccessLogs)
                .WithOne(ua => ua.ShortenedUrl)
                .HasForeignKey(ua => ua.ShortenedUrlId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
