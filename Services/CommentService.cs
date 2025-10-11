using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.Models;
using NexusGram.DTOs;

namespace NexusGram.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> AddCommentAsync(int userId, int postId, string content, int? parentCommentId = null)
        {
            // Check if post exists
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                throw new Exception("Post not found");

            // Check if parent comment exists (for replies)
            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(parentCommentId.Value);
                if (parentComment == null)
                    throw new Exception("Parent comment not found");
            }

            var comment = new Comment
            {
                UserId = userId,
                PostId = postId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

            if (comment == null)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<CommentResponse>> GetCommentsByPostAsync(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.ParentCommentId == null) // Only top-level comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    ProfilePicture = c.User.ProfilePicture,
                    ParentCommentId = c.ParentCommentId,
                    Replies = c.Replies.Select(r => new CommentResponse
                    {
                        Id = r.Id,
                        Content = r.Content,
                        CreatedAt = r.CreatedAt,
                        UserId = r.UserId,
                        Username = r.User.Username,
                        ProfilePicture = r.User.ProfilePicture,
                        ParentCommentId = r.ParentCommentId
                    }).ToList()
                })
                .ToListAsync();

            return comments;
        }

        public async Task<CommentResponse> GetCommentAsync(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new Exception("Comment not found");

            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                Username = comment.User.Username,
                ProfilePicture = comment.User.ProfilePicture,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task<int> GetCommentCountAsync(int postId)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId);
        }
    }
}