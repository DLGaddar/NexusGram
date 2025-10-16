using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.DTOs;
using NexusGram.Models;
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
        private readonly ApplicationDbContext _context;

        public PostsController(IPostService postService, ApplicationDbContext context)
        {
            _postService = postService;
            _context = context;
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
        public async Task<ActionResult<List<PostResponse>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // 🚨 Artık page ve pageSize parametrelerini kullanıyoruz
                var posts = await _postService.GetFeedAsync(userId, page, pageSize); 
                
                // DİKKAT: Frontend'in PaginatedResponse objesine dönmesi için 
                // buradaki geri dönüş tipini kontrol edin veya DTO kullanın.
                // Şimdilik sadece posts listesini döndürdüğünüzü varsayıyorum.
                return Ok(posts); 
            }
            catch (Exception ex)
            {
                // ...
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

[HttpPost("create-test-posts")]
[AllowAnonymous] // Login gerektirmeden çalışsın
public async Task<ActionResult> CreateTestPosts()
{
    try
    {
        // Test kullanıcısını bul veya oluştur
        var testUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        if (testUser == null)
        {
            testUser = new User 
            { 
                Username = "testuser", 
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();
        }

        // Test postları oluştur - SADECE MEVCUT PROPERTY'LERİ KULLAN
        var testPosts = new List<Post>
        {
            new Post 
            { 
                ImageUrl = "https://picsum.photos/600/600?1", 
                Caption = "Harika bir gün! 🌞",
                Location = "İstanbul, Türkiye",
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Post 
            { 
                ImageUrl = "https://picsum.photos/600/600?2", 
                Caption = "Doğa harikası 🏞️",
                Location = "Kapadokya, Türkiye", 
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Post 
            { 
                ImageUrl = "https://picsum.photos/600/600?3", 
                Caption = "Kahve molası ☕",
                Location = "Kadıköy, İstanbul",
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            }
        };
        
        _context.Posts.AddRange(testPosts);
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            message = $"{testPosts.Count} test postu başarıyla eklendi",
            posts = testPosts.Select(p => new {
                id = p.Id,
                imageUrl = p.ImageUrl,
                caption = p.Caption,
                location = p.Location,
                userId = p.UserId,
                createdAt = p.CreatedAt
            })
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = $"Test postları oluşturulurken hata: {ex.Message}" });
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