using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NexusGram.Data;
using NexusGram.Models;

namespace NexusGram.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public AuthService(ApplicationDbContext context, IPasswordHasher passwordHasher, IConfiguration configuration, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists");

            // Create user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            var jwtToken = _jwtService.GenerateToken(user.Id, user.Username);

            return user;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
        
            var token = GenerateJwtToken(user);
            var jwtToken = _jwtService.GenerateToken(user.Id, user.Username);
            return new LoginResponse
            {
                Token = token,
                Message = "Login successful", 
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                UserId = user.Id
            };
        }

public string GenerateJwtToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    
    var keyString = _configuration["Jwt:Key"] ?? 
                    throw new InvalidOperationException("JWT Secret Key is not configured.");
    
    var key = Encoding.UTF8.GetBytes(keyString);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddHours(3),
        Issuer = _configuration["Jwt:Issuer"],
        Audience = _configuration["Jwt:Audience"],
        
        // 🚨 KRİTİK KONTROL: SigningCredentials
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature // 👈 Algoritmanın doğru ayarlandığından emin olun!
        )
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
    }
}