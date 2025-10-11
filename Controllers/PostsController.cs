using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.DTOs;
using NexusGram.Services;
using System.Security.Claims;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PostResponse>> CreatePost([FromForm] CreatePostFormRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var createRequest = new CreatePostRequest
                {
                    Caption = request.Caption,
                    Location = request.Location
                };
                
                var post = await _postService.CreatePostAsync(userId, createRequest, request.Image);
                
                var response = new PostResponse
                {
                    Id = post.Id,
                    ImageUrl = post.ImageUrl,
                    Caption = post.Caption,
                    Location = post.Location,
                    CreatedAt = post.CreatedAt,
                    UserId = post.UserId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("feed")]
        public async Task<ActionResult<List<PostResponse>>> GetFeed()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var posts = await _postService.GetFeedAsync(userId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult<PostResponse>> GetPost(int postId)
        {
            try
            {
                var post = await _postService.GetPostAsync(postId);
                return Ok(post);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{postId}")]
        public async Task<ActionResult> DeletePost(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _postService.DeletePostAsync(postId, userId);
                
                if (!result)
                    return NotFound(new { message = "Post not found or you don't have permission" });

                return Ok(new { message = "Post deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CreatePostFormRequest
    {
        public string Caption { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public IFormFile Image { get; set; } = null!;
    }
}