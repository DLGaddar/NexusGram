namespace NexusGram.Models
{
    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Comment Comment { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}