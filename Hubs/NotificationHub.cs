using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NexusGram.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        public async Task SendLikeNotification(string targetUserId, string postId, string likerUsername)
        {
            await Clients.Group($"user-{targetUserId}").SendAsync("ReceiveLikeNotification", new
            {
                PostId = postId,
                LikerUsername = likerUsername,
                Message = $"{likerUsername} gönderini beğendi",
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendCommentNotification(string targetUserId, string postId, string commenterUsername)
        {
            await Clients.Group($"user-{targetUserId}").SendAsync("ReceiveCommentNotification", new
            {
                PostId = postId,
                CommenterUsername = commenterUsername,
                Message = $"{commenterUsername} gönderine yorum yaptı",
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendFollowNotification(string targetUserId, string followerUsername)
        {
            await Clients.Group($"user-{targetUserId}").SendAsync("ReceiveFollowNotification", new
            {
                FollowerUsername = followerUsername,
                Message = $"{followerUsername} seni takip etti",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}