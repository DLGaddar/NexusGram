using NexusGram.Models;

namespace NexusGram.Services
{
    public interface ILikeService
    {
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);
        Task<bool> IsPostLikedByUserAsync(int userId, int postId);
        Task<int> GetLikeCountAsync(int postId);
    }
}