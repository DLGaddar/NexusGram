using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.Models;
using NexusGram.DTOs;

namespace NexusGram.Services
{
    // Arayüzdeki tüm metotları uygular.
    public class FollowService : IFollowService 
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
                throw new Exception("Cannot follow yourself");

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
                return false;

            var userToFollow = await _context.Users.FindAsync(followingId);
            if (userToFollow == null)
                throw new Exception("User not found");

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null)
                return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<List<UserProfileResponse>> GetFollowersAsync(int userId, int requestingUserId) 
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                .Select(f => new UserProfileResponse
                {
                    Id = f.Follower.Id,
                    Username = f.Follower.Username,
                    ProfilePicture = f.Follower.ProfilePicture ?? string.Empty,
                    Bio = f.Follower.Bio ?? string.Empty,
                    IsPrivate = f.Follower.IsPrivate,
                    // KRİTİK: requestingUserId'yi kullanarak IsFollowing kontrolü yapılıyor.
                    IsFollowing = _context.Follows.Any(uf => uf.FollowerId == requestingUserId && uf.FollowingId == f.Follower.Id) 
                })
                .ToListAsync();
        
            return followers;
        }

        public async Task<List<UserProfileResponse>> GetFollowingAsync(int userId, int requestingUserId) 
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .Select(f => new UserProfileResponse
                {
                    Id = f.Following.Id,
                    Username = f.Following.Username,
                    ProfilePicture = f.Following.ProfilePicture ?? string.Empty,
                    Bio = f.Following.Bio ?? string.Empty,
                    IsPrivate = f.Following.IsPrivate,
                    // KRİTİK: requestingUserId'yi kullanarak IsFollowing kontrolü yapılıyor.
                    IsFollowing = _context.Follows.Any(uf => uf.FollowerId == requestingUserId && uf.FollowingId == f.Following.Id) 
                })
                .ToListAsync();
        
            return following;
        }

        public async Task<int> GetFollowerCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId);
        }
    }
}