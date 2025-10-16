using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
using NexusGram.DTOs;
using System.Security.Claims;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<ActionResult<DTOs.CommentResponse>> AddComment(int postId, [FromBody] AddCommentRequest request) // 🔥 Açıkça DTOs.CommentResponse
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized();

                var comment = await _commentService.AddCommentAsync(userId, postId, request.Content, request.ParentCommentId);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while adding comment" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<DTOs.CommentResponse>>> GetComments(int postId) // 🔥 Açıkça DTOs.CommentResponse
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var comments = await _commentService.GetCommentsByPostAsync(postId, currentUserId != null ? int.Parse(currentUserId) : (int?)null);
                return Ok(comments);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving comments" });
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<ActionResult> DeleteComment(int postId, int commentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized();

                var result = await _commentService.DeleteCommentAsync(commentId, userId);

                if (!result)
                    return NotFound(new { message = "Comment not found or you don't have permission" });

                return Ok(new { message = "Comment deleted successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while deleting comment" });
            }
        }

        [HttpPost("{commentId}/like")]
        public async Task<ActionResult> LikeComment(int postId, int commentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized();

                var result = await _commentService.LikeCommentAsync(commentId, userId);

                if (!result)
                    return BadRequest(new { message = "Comment already liked" });

                var likeCount = await _commentService.GetCommentLikeCountAsync(commentId);
                return Ok(new { likeCount, message = "Comment liked successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while liking comment" });
            }
        }

        [HttpDelete("{commentId}/like")]
        public async Task<ActionResult> UnlikeComment(int postId, int commentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized();

                var result = await _commentService.UnlikeCommentAsync(commentId, userId);

                if (!result)
                    return BadRequest(new { message = "Comment not liked" });

                var likeCount = await _commentService.GetCommentLikeCountAsync(commentId);
                return Ok(new { likeCount, message = "Comment unliked successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while unliking comment" });
            }
        }
    }
}