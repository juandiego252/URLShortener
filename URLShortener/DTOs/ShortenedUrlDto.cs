namespace URLShortener.DTOs
{
    public class ShortenedUrlDto
    {
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public string ShortenedUrl { get; set; }
        public int AccessCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
