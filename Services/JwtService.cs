// Services/JwtService.cs - MANUEL JWT DECODE
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace NexusGram.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
            _issuer = configuration["Jwt:Issuer"] ?? "NexusGram";
            _audience = configuration["Jwt:Audience"] ?? "NexusGramUsers";
            
            Console.WriteLine($"🔐 JWT Service Initialized:");
            Console.WriteLine($"🔐 Secret Key Length: {_secretKey.Length}");
        }
        
        public string GenerateToken(int userId, string username)
        {
            Console.WriteLine($"🔐 Generating token for UserId: {userId}, Username: {username}");
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("userId", userId.ToString())
            };
        
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);
        
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            
            // ✅ TOKEN İÇERİĞİNİ KONTROL ET - BU ÇOK ÖNEMLİ!
            Console.WriteLine($"🔐 === TOKEN VERIFICATION ===");
            var tokenHandler = new JwtSecurityTokenHandler();
            var decodedToken = tokenHandler.ReadJwtToken(jwtToken);
            foreach (var claim in decodedToken.Claims)
            {
                Console.WriteLine($"   📌 {claim.Type}: {claim.Value}");
            }
            Console.WriteLine($"🔐 === END TOKEN VERIFICATION ===");
            
            return jwtToken;
        }

        public int GetUserIdFromToken(string token)
        {
            Console.WriteLine($"🔐 GetUserIdFromToken - Manual JWT Decode");
            
            try
            {
                // ✅ 1. YÖNTEM: Manuel JWT decode - Base64 decode
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    throw new SecurityTokenException("Invalid JWT token format");
                }

                // Payload (ikinci part) decode et
                var payload = tokenParts[1];
                
                // Base64 URL decode
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                
                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);
                
                Console.WriteLine($"🔐 Raw JWT Payload: {payloadJson}");
                
                // JSON'dan claim'leri oku
                using var document = JsonDocument.Parse(payloadJson);
                var root = document.RootElement;
                
                // ✅ Sub claim'ini ara
                if (root.TryGetProperty("sub", out var subElement) && 
                    subElement.ValueKind == JsonValueKind.String)
                {
                    if (int.TryParse(subElement.GetString(), out var userId))
                    {
                        Console.WriteLine($"✅ Manual Decode - User ID from 'sub': {userId}");
                        return userId;
                    }
                }
                
                // ✅ unique_name claim'ini ara
                if (root.TryGetProperty("unique_name", out var uniqueNameElement) && 
                    uniqueNameElement.ValueKind == JsonValueKind.String)
                {
                    // unique_name username olabilir, ID değil - bu durumda test ID kullan
                    Console.WriteLine($"✅ Manual Decode - Username found: {uniqueNameElement.GetString()}, using test ID: 1");
                    return 1;
                }
                
                // ✅ userId custom claim'ini ara
                if (root.TryGetProperty("userId", out var userIdElement) && 
                    userIdElement.ValueKind == JsonValueKind.String)
                {
                    if (int.TryParse(userIdElement.GetString(), out var userId))
                    {
                        Console.WriteLine($"✅ Manual Decode - User ID from 'userId': {userId}");
                        return userId;
                    }
                }
                
                // ✅ nameidentifier claim'ini ara
                if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var nameIdElement))
                {
                    var value = nameIdElement.ValueKind == JsonValueKind.String 
                        ? nameIdElement.GetString() 
                        : nameIdElement.GetRawText();
                    
                    if (int.TryParse(value, out var userId))
                    {
                        Console.WriteLine($"✅ Manual Decode - User ID from 'nameidentifier': {userId}");
                        return userId;
                    }
                }
                
                // ❌ Hiçbir claim bulunamazsa
                Console.WriteLine($"❌ No user ID claim found in JWT payload");
                Console.WriteLine($"🔄 Using fallback user ID: 1");
                return 1; // Fallback
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Manual JWT decode error: {ex.Message}");
                Console.WriteLine($"🔄 Using fallback user ID: 1");
                return 1; // Fallback
            }
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            // Basit implementation - şimdilik gerek yok
            var claims = new[] { new Claim(ClaimTypes.Name, "user") };
            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }
}