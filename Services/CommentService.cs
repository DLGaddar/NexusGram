using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.DTOs;
using NexusGram.Models;

namespace NexusGram.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommentResponse> AddCommentAsync(int userId, int postId, string content, int? parentCommentId = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment content cannot be empty");

            if (content.Length > 500)
                throw new ArgumentException("Comment cannot exceed 500 characters");

            // Check if post exists
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                throw new ArgumentException("Post not found");

            // Check parent comment if replying
            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId && c.PostId == postId);
                if (parentComment == null)
                    throw new ArgumentException("Parent comment not found");
            }

            var comment = new Comment
            {
                Content = content.Trim(),
                UserId = userId,
                PostId = postId,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Yeni eklenen comment'i user bilgisiyle birlikte getir
            var newComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return await MapToCommentResponse(newComment!, userId);
        }

        public async Task<List<CommentResponse>> GetCommentsByPostAsync(int postId, int? currentUserId = null)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Likes)
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var response = new List<CommentResponse>();

            foreach (var comment in comments)
            {
                response.Add(await MapToCommentResponse(comment, currentUserId));
            }

            return response;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || comment.UserId != userId)
                return false;

            // If comment has replies, soft delete (just hide content)
            if (comment.Replies.Any())
            {
                comment.Content = "[deleted]";
                comment.UpdatedAt = DateTime.UtcNow;
                _context.Comments.Update(comment);
            }
            else
            {
                _context.Comments.Remove(comment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LikeCommentAsync(int commentId, int userId)
        {
            // Check if already liked
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (existingLike != null)
                return false;

            var like = new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            _context.CommentLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeCommentAsync(int commentId, int userId)
        {
            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (like == null)
                return false;

            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCommentLikeCountAsync(int commentId)
        {
            return await _context.CommentLikes
                .CountAsync(cl => cl.CommentId == commentId);
        }

        public async Task<bool> IsCommentLikedByUserAsync(int commentId, int userId)
        {
            return await _context.CommentLikes
                .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
        }

        private async Task<CommentResponse> MapToCommentResponse(Comment comment, int? currentUserId = null)
        {
            var likeCount = await GetCommentLikeCountAsync(comment.Id);
            var isLiked = currentUserId.HasValue && await IsCommentLikedByUserAsync(comment.Id, currentUserId.Value);

            var response = new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                ParentCommentId = comment.ParentCommentId,
                LikeCount = likeCount,
                IsLikedByCurrentUser = isLiked,
                User = new UserProfileDto
                {
                    Id = comment.User.Id.ToString(),
                    UserName = comment.User.Username,
                    ProfilePicture = comment.User.ProfilePicture ?? "/images/default-avatar.png"
                },
                Replies = new List<CommentResponse>()
            };

            // Map replies recursively
            foreach (var reply in comment.Replies.OrderBy(r => r.CreatedAt))
            {
                response.Replies.Add(await MapToCommentResponse(reply, currentUserId));
            }

            return response;
        }
    }
}