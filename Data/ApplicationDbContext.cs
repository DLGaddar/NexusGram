using Microsoft.EntityFrameworkCore;
using NexusGram.Models;

namespace NexusGram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Follow relationship
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique();
                
            // Like relationship  
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();
                
            // Comment relationships
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        
            // Post relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        
            // Story relationship
            modelBuilder.Entity<Story>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        
            // Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 COMMENTLIKE CONFIGURATION DÜZELTMESİ - CASCADE HATASI ÇÖZÜMÜ
            modelBuilder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(cl => cl.Id);
                
                entity.HasOne(cl => cl.Comment)
                    .WithMany(c => c.Likes)
                    .HasForeignKey(cl => cl.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(cl => cl.User)
                    .WithMany(u => u.CommentLikes)
                    .HasForeignKey(cl => cl.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // 🔥 CASCADE → RESTRICT (CRITICAL FIX)
                    
                entity.HasIndex(cl => new { cl.CommentId, cl.UserId })
                    .IsUnique();
            });
        }
    }
}