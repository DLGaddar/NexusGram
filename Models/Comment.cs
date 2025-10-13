namespace NexusGram.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; } // For replies
        public string Title { get; set; } = string.Empty;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = new();
    }
}