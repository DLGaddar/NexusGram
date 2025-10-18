// Services/IUserService.cs
using NexusGram.Models;

namespace NexusGram.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User?> RegisterAsync(string username, string email, string password); // ✅ Basitleştir
        Task<User?> GetUserByIdAsync(int id);
    }
}