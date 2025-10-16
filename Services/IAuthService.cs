using NexusGram.Models;

namespace NexusGram.Services
{
    public interface IAuthService
    {
      Task<User> RegisterAsync(string username, string email, string password);
      Task<LoginResponse> LoginAsync(string username, string password); // AuthResponse → LoginResponse
      string GenerateJwtToken(User user);
    }
}