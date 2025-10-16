namespace NexusGram.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public int UserId { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        
        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}