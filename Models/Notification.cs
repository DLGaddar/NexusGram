namespace NexusGram.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // like, comment, follow, mention
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; } // Receiver
        public int ActorId { get; set; } // Who triggered the notification
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!; // Receiver
        public User Actor { get; set; } = null!; // Trigger user
        public Post? Post { get; set; }
        public Comment? Comment { get; set; }
    }
}