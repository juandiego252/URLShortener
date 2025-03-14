namespace URLShortener.DTOs
{
    public class ShortenedUrlCacheDto
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
}
