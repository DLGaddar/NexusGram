using NexusGram.DTOs;

namespace NexusGram.Services
{
    public interface IFollowService
    {
        Task<bool> FollowUserAsync(int followerId, int followingId);
        Task<bool> UnfollowUserAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<List<UserProfileResponse>> GetFollowersAsync(int userId);
        Task<List<UserProfileResponse>> GetFollowingAsync(int userId);
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