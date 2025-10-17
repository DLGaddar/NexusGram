using NexusGram.DTOs;
using NexusGram.Services;

namespace NexusGram.Services
{
    public interface IFollowService
    {
        Task<bool> FollowUserAsync(int followerId, int followingId);
        Task<bool> UnfollowUserAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<List<UserProfileResponse>> GetFollowersAsync(int userId, int requestingUserId);
        Task<List<UserProfileResponse>> GetFollowingAsync(int userId, int requestingUserId);
        Task<int> GetFollowerCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public bool IsFollowing { get; set; }
        public bool IsPrivate { get; set; }
    }
}