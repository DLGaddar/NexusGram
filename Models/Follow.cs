using System.ComponentModel.DataAnnotations;

namespace NexusGram.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int FollowerId { get; set; } // Takip eden
        public int FollowingId { get; set; } = null!; // Takip edilen
        
        // Navigation properties
        public User Follower { get; set; } = null!;
        public User Following { get; set; } = null!;
    }
}