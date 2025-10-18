using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.DTOs;
using NexusGram.Models;

namespace NexusGram.Services;

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
            throw new InvalidOperationException("You cannot follow yourself.");

        if (await IsFollowingAsync(followerId, followingId))
            return false;

        var userToFollow = await _context.Users.FindAsync(followingId);
        if (userToFollow == null)
            throw new KeyNotFoundException("User to follow not found.");

        var follow = new Follow { FollowerId = followerId, FollowingId = followingId };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (follow == null) return false;

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
        var followingIdsSet = await _context.Follows
            .Where(f => f.FollowerId == requestingUserId)
            .Select(f => f.FollowingId)
            .ToHashSetAsync();

        return await _context.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => new UserProfileResponse
            {
                Id = f.Follower.Id,
                Username = f.Follower.Username,
                ProfilePicture = f.Follower.ProfilePicture ?? string.Empty,
                Bio = f.Follower.Bio ?? string.Empty,
                IsPrivate = f.Follower.IsPrivate,
                IsFollowing = followingIdsSet.Contains(f.Follower.Id)
            })
            .ToListAsync();
    }

    public async Task<List<UserProfileResponse>> GetFollowingAsync(int userId, int requestingUserId)
    {
        var followingIdsSet = await _context.Follows
            .Where(f => f.FollowerId == requestingUserId)
            .Select(f => f.FollowingId)
            .ToHashSetAsync();

        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => new UserProfileResponse
            {
                Id = f.Following.Id,
                Username = f.Following.Username,
                ProfilePicture = f.Following.ProfilePicture ?? string.Empty,
                Bio = f.Following.Bio ?? string.Empty,
                IsPrivate = f.Following.IsPrivate,
                IsFollowing = followingIdsSet.Contains(f.Following.Id)
            })
            .ToListAsync();
    }

    public async Task<int> GetFollowerCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerId == userId);
    }
}