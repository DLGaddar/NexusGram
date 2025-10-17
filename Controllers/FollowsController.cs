// Controllers/FollowsController.cs (Düzeltilmiş Versiyon)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
using System.IdentityModel.Tokens.Jwt; // JwtRegisteredClaimNames için gerekli

namespace NexusGram.Controllers
{
    [ApiController]
    // {userId} routing'den kaldırıldı, FollowsController genel bir servis olarak çalışmalı.
    [Route("api/[controller]")] 
    [Authorize]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }
        
        // 🚨 YENİ: ID'yi güvenilir şekilde çeken yardımcı metot
        private int GetFollowerIdFromClaims()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // [Authorize] olduğu için buraya düşmek token hatasıdır.
                throw new UnauthorizedAccessException("Geçersiz yetkilendirme: Kullanıcı ID'si bulunamadı.");
            }
            return userId;
        }


        // ----------------------------------------------------
        // POST: api/follows/{userIdToFollow} (TAKİP ET / BIRAK)
        // ----------------------------------------------------
        // Controller'ı basitleştirmek ve tek bir işlevi yerine getirmek için ToggleFollowAsync kullanıyoruz.
        [HttpPost("{userIdToFollow}")]
        public async Task<ActionResult> ToggleFollow(int userIdToFollow)
        {
            try
            {
                var followerId = GetFollowerIdFromClaims();
                
                if (followerId == userIdToFollow)
                {
                    return BadRequest(new { message = "Kendinizi takip edemezsiniz." });
                }

                var isFollowing = await _followService.ToggleFollowAsync(followerId, userIdToFollow);

                return Ok(new 
                { 
                    isFollowing = isFollowing, 
                    message = isFollowing ? "Kullanıcı takip edildi." : "Kullanıcı takibi bırakıldı."
                });
            }
            catch (Exception ex)
            {
                // KeyNotFoundException vs. burada yakalanabilir.
                return BadRequest(new { message = ex.Message });
            }
        }

        // ----------------------------------------------------
        // GET: api/follows/followers/{userId} (TAKİPÇİLER)
        // ----------------------------------------------------
        [HttpPost("{userIdToFollow}")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowUser(int userIdToFollow)
        {
            try
            {
                var followerId = GetFollowerIdFromClaims();
                var result = await _followService.FollowUserAsync(followerId, userIdToFollow); // 👈 FollowUserAsync çağrılıyor
                
                if (!result)
                    return Conflict(new { message = "Kullanıcı zaten takip ediliyor." }); // 409 Conflict
                
                return Ok(new { message = "Kullanıcı takip edildi.", isFollowing = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{userIdToFollow}")]
        public async Task<IActionResult> UnfollowUser(int userIdToFollow)
        {
            try
            {
                var followerId = GetFollowerIdFromClaims();
                var result = await _followService.UnfollowUserAsync(followerId, userIdToFollow); // 👈 UnfollowUserAsync çağrılıyor
                
                if (!result)
                    return NotFound(new { message = "Kullanıcı zaten takip edilmiyor." }); // 404 Not Found
        
                return Ok(new { message = "Takip bırakıldı.", isFollowing = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ----------------------------------------------------
        // GET: api/follows/following/{userId} (TAKİP EDİLENLER)
        // ----------------------------------------------------
        [HttpGet("following/{userId}")]
        [AllowAnonymous] // Takip edilenler listesini herkes görebilir
        public async Task<ActionResult<List<UserProfileResponse>>> GetFollowing(int userId)
        {
             try
            {
                var requestingUserId = User.Identity?.IsAuthenticated == true ? GetFollowerIdFromClaims() : 0;

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