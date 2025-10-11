using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.Models;

namespace NexusGram.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LikePostAsync(int userId, int postId)
        {
            // Check if already liked
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (existingLike != null)
                return false; // Already liked

            // Check if post exists
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                throw new Exception("Post not found");

            var like = new Like
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnlikePostAsync(int userId, int postId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (like == null)
                return false; // Not liked

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsPostLikedByUserAsync(int userId, int postId)
        {
            return await _context.Likes
                .AnyAsync(l => l.UserId == userId && l.PostId == postId);
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.Likes
                .CountAsync(l => l.PostId == postId);
        }
    }
}