using NexusGram.Models;

namespace NexusGram.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string email, string password);
        Task<User> LoginAsync(string username, string password);
        string GenerateJwtToken(User user);
    }
}