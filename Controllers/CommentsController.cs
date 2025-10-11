using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
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
        public async Task<ActionResult> AddComment(int postId, [FromBody] AddCommentRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var comment = await _commentService.AddCommentAsync(userId, postId, request.Content, request.ParentCommentId);
                
                var response = new CommentResponse
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UserId = comment.UserId,
                    ParentCommentId = comment.ParentCommentId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentResponse>>> GetComments(int postId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByPostAsync(postId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<ActionResult> DeleteComment(int postId, int commentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _commentService.DeleteCommentAsync(commentId, userId);

                if (!result)
                    return NotFound(new { message = "Comment not found or you don't have permission" });

                return Ok(new { message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class AddCommentRequest
    {
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
    }
}