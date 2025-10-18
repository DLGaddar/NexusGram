using System.Security.Claims;

namespace NexusGram.Services
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string username);
        ClaimsPrincipal ValidateToken(string token);
        int GetUserIdFromToken(string token);
    }
}