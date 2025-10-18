namespace NexusGram.Models
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public required string Email { get; set; }
    }

    public class RegisterResponse
    {
        public required string Token { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}