namespace NexusGram.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public List<Like> Likes { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }
}