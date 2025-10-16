using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.Models;
using NexusGram.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace NexusGram.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostService(ApplicationDbContext context, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Post> CreatePostAsync(int userId, CreatePostRequest request, IFormFile image)
        {
            // ✅ FILE UPLOAD GÜVENLİK KONTROLLERİ
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxSize = 10 * 1024 * 1024; // 10MB

            if (image.Length > maxSize)
                throw new Exception("Dosya boyutu çok büyük. Maksimum 10MB.");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new Exception("Geçersiz dosya türü. İzin verilenler: jpg, jpeg, png, gif");

            // ✅ DOSYA YÜKLEME
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "posts");
            
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // ✅ POST OLUŞTURMA
            var post = new Post
            {
                UserId = userId,
                ImageUrl = $"/uploads/posts/{fileName}",
                Caption = request.Caption,
                Location = request.Location,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<List<PostResponse>> GetFeedAsync(int userId, int page, int pageSize) 
        {
            // Input kontrolü eklenmesi iyi bir pratik.
            if (page < 1) page = 1; 
            if (pageSize < 1) pageSize = 10; // Varsayılan değer
        
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();
        
            var postIds = followingIds.Append(userId).ToList();
        
            var posts = await _context.Posts
                .Where(p => postIds.Contains(p.UserId))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                
                // 🚨 KRİTİK SAYFALAMA MANTIĞI EKLENDİ
                .Skip((page - 1) * pageSize) 
                .Take(pageSize) 
                
                .Select(p => new PostResponse
                {
                    // ... (Mevcut eşleme mantığınız)
                    Id = p.Id,
                    ImageUrl = p.ImageUrl,
                    Caption = p.Caption,
                    Location = p.Location,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    ProfilePicture = p.User.ProfilePicture,
                    LikeCount = p.Likes.Count,
                    CommentCount = p.Comments.Count,
                    IsLiked = p.Likes.Any(like => like.UserId == userId)
                })
                .ToListAsync();
        
            return posts;
        }

        public async Task<PostResponse> GetPostAsync(int postId)
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new Exception("Post not found");

            return new PostResponse
            {
                Id = post.Id,
                ImageUrl = post.ImageUrl,
                Caption = post.Caption,
                Location = post.Location,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                Username = post.User.Username,
                ProfilePicture = post.User.ProfilePicture,
                LikeCount = post.Likes.Count,
                CommentCount = post.Comments.Count,
                IsLiked = post.Likes.Any(like => like.UserId == userId)
            };
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
            
            if (post == null)
                return false;

            // ✅ DOSYAYI DA SİL
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                var filePath = Path.Combine(_environment.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}