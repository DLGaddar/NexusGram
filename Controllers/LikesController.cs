using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
using System.Security.Claims;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/[controller]")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost]
        public async Task<ActionResult> LikePost(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _likeService.LikePostAsync(userId, postId);

                if (!result)
                    return BadRequest(new { message = "Already liked or post not found" });

                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> UnlikePost(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _likeService.UnlikePostAsync(userId, postId);

                if (!result)
                    return BadRequest(new { message = "Post not liked" });

                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult> GetLikeStatus(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var isLiked = await _likeService.IsPostLikedByUserAsync(userId, postId);
                var likeCount = await _likeService.GetLikeCountAsync(postId);

                return Ok(new { isLiked, likeCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}