using System.ComponentModel.DataAnnotations.Schema;
namespace NexusGram.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image";

        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
        
        // Foreign key
        public int UserId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
}