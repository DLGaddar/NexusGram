using NexusGram.DTOs;
using NexusGram.Models;

namespace NexusGram.Services
{
    public interface ICommentService
    {
        Task<CommentResponse> AddCommentAsync(int userId, int postId, string content, int? parentCommentId = null);
        Task<List<CommentResponse>> GetCommentsByPostAsync(int postId, int? currentUserId = null);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<bool> LikeCommentAsync(int commentId, int userId);
        Task<bool> UnlikeCommentAsync(int commentId, int userId);
        Task<int> GetCommentLikeCountAsync(int commentId);
        Task<bool> IsCommentLikedByUserAsync(int commentId, int userId);
    }
}