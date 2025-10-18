// Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using NexusGram.Data;
using NexusGram.Models;
using BCrypt.Net;

namespace NexusGram.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            Console.WriteLine($"🔐 AUTHENTICATE: {username}");
            
            // 1. Önce username'i ara
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                Console.WriteLine($"❌ USER NOT FOUND: {username}");
                
                // Username bulunamazsa DLGaddar'ı ara
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "DLGaddar");
                
                if (user == null)
                {
                    // Hiçbiri yoksa ilk user'ı al
                    user = await _context.Users.FirstAsync();
                    Console.WriteLine($"🔐 Using first user: {user.Username}");
                }
                else
                {
                    Console.WriteLine($"🔐 Using DLGaddar as fallback");
                }
            }
        
            // ✅ HER ZAMAN BAŞARILI DÖN - TEST MODE
            Console.WriteLine($"✅ AUTHENTICATION SUCCESS: {user.Username}, ID: {user.Id}");
            return user;
        }

        public async Task<User?> RegisterAsync(string username, string email, string password)
        {
            // Username veya email kontrolü
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            
            if (existingUser != null) return null;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password, // Geçici - hash kullan!
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}