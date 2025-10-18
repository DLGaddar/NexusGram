using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
using NexusGram.DTOs;
using System.Security.Claims; // Bu using ifadesi NameIdentifier için gerekli

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }
        
        // Güvenilir şekilde JWT tokendan kullanıcı ID'sini çeken yardımcı metot
        private int GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier, genellikle JWT'deki 'sub' (subject) claim'ine karşılık gelir ve kullanıcı ID'sini tutar.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // [Authorize] attribute'u olduğu için normalde bu hatanın alınmaması gerekir.
                // Alınıyorsa, token'da bir sorun var demektir.
                throw new UnauthorizedAccessException("Kullanıcı kimliği token içerisinde bulunamadı.");
            }
            return userId;
        }

        // --- EYLEM METOTLARI ---

        // TAKİP ETMEK İÇİN
        // POST: api/follows/{userIdToFollow}
        [HttpPost("{userIdToFollow}")]
        public async Task<IActionResult> FollowUser(int userIdToFollow)
        {
            try
            {
                var followerId = GetCurrentUserId();
                var result = await _followService.FollowUserAsync(followerId, userIdToFollow);
                
                if (!result)
                    return Conflict(new { message = "Kullanıcı zaten takip ediliyor veya kendinizi takip etmeye çalışıyorsunuz." }); // 409 Conflict
                
                return Ok(new { message = "Kullanıcı başarıyla takip edildi.", isFollowing = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // TAKİBİ BIRAKMAK İÇİN
        // DELETE: api/follows/{userIdToFollow}
        [HttpDelete("{userIdToFollow}")]
        public async Task<IActionResult> UnfollowUser(int userIdToFollow)
        {
            try
            {
                var followerId = GetCurrentUserId();
                var result = await _followService.UnfollowUserAsync(followerId, userIdToFollow);
                
                if (!result)
                    return NotFound(new { message = "Takip ilişkisi bulunamadı." }); // 404 Not Found
            
                return Ok(new { message = "Takip başarıyla bırakıldı.", isFollowing = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // BİR KULLANICININ TAKİPÇİLERİNİ GETİRMEK İÇİN
        // GET: api/follows/followers/{userId}
        [HttpGet("followers/{userId}")]
        [AllowAnonymous] // Herkesin görmesine izin ver (isteğe bağlı)
        public async Task<ActionResult<List<UserProfileResponse>>> GetFollowers(int userId)
        {
             try
            {
                // İstek atan kullanıcı login olmuşsa onun ID'sini al, olmamışsa 0 olarak yolla.
                // Bu, servisin "IsFollowing" durumunu doğru hesaplaması için gereklidir.
                var requestingUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : 0;

                var followers = await _followService.GetFollowersAsync(userId, requestingUserId);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // BİR KULLANICININ TAKİP ETTİKLERİNİ GETİRMEK İÇİN
        // GET: api/follows/following/{userId}
        [HttpGet("following/{userId}")]
        [AllowAnonymous] // Herkesin görmesine izin ver (isteğe bağlı)
        public async Task<ActionResult<List<UserProfileResponse>>> GetFollowing(int userId)
        {
             try
            {
                var requestingUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : 0;
                var following = await _followService.GetFollowingAsync(userId, requestingUserId);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}