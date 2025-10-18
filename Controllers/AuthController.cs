using Microsoft.AspNetCore.Mvc;
using NexusGram.Models;
using NexusGram.Services;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"🔐 LOGIN ATTEMPT: {request.Username}");
                
                var user = await _userService.AuthenticateAsync(request.Username, request.Password);
                if (user == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                var token = _jwtService.GenerateToken(user.Id, user.Username);
                
                Console.WriteLine($"🔐 LOGIN SUCCESS - UserId: {user.Id}, Token: {token.Substring(0, 50)}...");
                
                return Ok(new LoginResponse 
                { 
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LOGIN ERROR: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password);
                if (user == null)
                    return BadRequest(new { message = "Username or email already exists" });

                var token = _jwtService.GenerateToken(user.Id, user.Username);
                
                return Ok(new RegisterResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}