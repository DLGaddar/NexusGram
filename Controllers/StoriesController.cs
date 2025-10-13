// Controllers/StoriesController.cs - DÜZELTİLMİŞ
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NexusGram.Data;
using NexusGram.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StoriesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromForm] CreateStoryRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                if (request.Image == null || request.Image.Length == 0)
                    return BadRequest(new { message = "Image is required" });

                // Resim kaydetme
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "stories");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/stories/{fileName}";

                // Story oluştur
                var story = new Story
                {
                    UserId = userId, // ✅ int
                    ImageUrl = imageUrl,
                    Caption = request.Caption,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    id = story.Id, 
                    imageUrl = story.ImageUrl,
                    caption = story.Caption,
                    createdAt = story.CreatedAt,
                    expiresAt = story.ExpiresAt,
                    userId = story.UserId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetStoriesFeed()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                // Takip edilen kullanıcıların story'lerini getir
                var followingIds = await _context.Follows
                    .Where(f => f.FollowerId == userId) // ✅ int karşılaştırma
                    .Select(f => f.FollowingId)
                    .ToListAsync();

                // Kendi story'lerini de ekle
                followingIds.Add(userId);

                var stories = await _context.Stories
                    .Include(s => s.User)
                    .Where(s => followingIds.Contains(s.UserId) && 
                                s.CreatedAt > DateTime.UtcNow.AddHours(-24))
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new
                    {
                        id = s.Id,
                        imageUrl = s.ImageUrl,
                        caption = s.Caption,
                        createdAt = s.CreatedAt,
                        expiresAt = s.ExpiresAt,
                        userId = s.UserId,
                        username = s.User.Username,
                        profilePicture = s.User.ProfilePicture
                    })
                    .ToListAsync();

                return Ok(stories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                var story = await _context.Stories
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId); // ✅ int karşılaştırma

                if (story == null)
                    return NotFound(new { message = "Story not found" });

                // Resmi sil
                var filePath = Path.Combine(_environment.WebRootPath, story.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Story deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}