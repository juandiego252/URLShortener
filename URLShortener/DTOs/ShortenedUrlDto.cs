namespace URLShortener.DTOs
{
    public class ShortenedUrlDto
    {
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public string ShortenedUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
