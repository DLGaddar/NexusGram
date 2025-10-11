using NexusGram.Models;
using NexusGram.DTOs;

namespace NexusGram.Services
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(int userId, CreatePostRequest request, IFormFile image);
        Task<List<PostResponse>> GetFeedAsync(int userId);
        Task<PostResponse> GetPostAsync(int postId);
        Task<bool> DeletePostAsync(int postId, int userId);
    }
}