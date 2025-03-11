using System.ComponentModel.DataAnnotations;

namespace URLShortener.Models
{
    public class ShortenedUrl
    {
        public int Id { get; set; }
        [Required]
        public string OriginalUrl { get; set; }
        [Required]
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int AccessCount { get; set; } = 0;
        public DateTime? LastAccessedAt { get; set; }

        public virtual ICollection<UrlAccess> AccessLogs{ get; set; }
    }
}
