using System.ComponentModel.DataAnnotations.Schema;

namespace URLShortener.Models
{
    public class UrlAccess
    {
        public int Id { get; set; }

        [ForeignKey("ShortenedUrl")]
        public int ShortenedUrlId { get; set; }
        public virtual ShortenedUrl ShortenedUrl { get; set; }

        public DateTime AccessedAt { get; set; }

        public string UserAgent { get; set; }
    }
}
