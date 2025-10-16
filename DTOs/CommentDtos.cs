namespace NexusGram.DTOs
{
    public class AddCommentRequest
    {
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
    }

    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public UserProfileDto User { get; set; } = null!;
        public List<CommentResponse> Replies { get; set; } = new();
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
    }
}