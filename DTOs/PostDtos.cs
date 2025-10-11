namespace NexusGram.DTOs
{
    public class CreatePostRequest
    {
        public string Caption { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class PostResponse
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}