using System.ComponentModel.DataAnnotations;

namespace URLShortener.Models
{
    public class ShortenedUrl
    {
        public ShortenedUrl()
        {
            // Inicializar la colección al crear una nueva instancia
            AccessLogs = new List<UrlAccess>();
        }
        public int Id { get; set; }
        [Required]
        public string OriginalUrl { get; set; }
        [Required]
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int AccessCount { get; set; } = 0;
        public DateTime? LastAccessedAt { get; set; }

        public virtual ICollection<UrlAccess> AccessLogs{ get; set; } = new List<UrlAccess>();
    }
}
