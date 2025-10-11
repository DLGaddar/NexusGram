namespace NexusGram.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image"; // image, video
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
        
        // Foreign key
        public int UserId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
}