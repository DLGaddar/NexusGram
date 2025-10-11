using NexusGram.Models;
using NexusGram.DTOs;

namespace NexusGram.Services
{
    public interface ICommentService
    {
        Task<Comment> AddCommentAsync(int userId, int postId, string content, int? parentCommentId = null);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<List<CommentResponse>> GetCommentsByPostAsync(int postId);
        Task<CommentResponse> GetCommentAsync(int commentId);
        Task<int> GetCommentCountAsync(int postId);
    }

    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public List<CommentResponse> Replies { get; set; } = new();
    }
}