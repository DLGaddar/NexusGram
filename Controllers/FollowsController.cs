using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGram.Services;
using System.Security.Claims;

namespace NexusGram.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [Authorize]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost]
        public async Task<ActionResult> FollowUser(int userId)
        {
            try
            {
                var followerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _followService.FollowUserAsync(followerId, userId);

                if (!result)
                    return BadRequest(new { message = "Already following or user not found" });

                return Ok(new { message = "User followed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> UnfollowUser(int userId)
        {
            try
            {
                var followerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _followService.UnfollowUserAsync(followerId, userId);

                if (!result)
                    return BadRequest(new { message = "Not following this user" });

                return Ok(new { message = "User unfollowed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("followers")]
        public async Task<ActionResult> GetFollowers(int userId)
        {
            try
            {
                var followers = await _followService.GetFollowersAsync(userId);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("following")]
        public async Task<ActionResult> GetFollowing(int userId)
        {
            try
            {
                var following = await _followService.GetFollowingAsync(userId);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}