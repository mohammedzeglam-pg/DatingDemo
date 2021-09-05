using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace API.Data
{
  public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole,
  IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


      //Like Entity config
      base.OnModelCreating(modelBuilder);
      _ = modelBuilder.Entity<AppUser>().HasMany(ur => ur.UserRoles).WithOne(u => u.User)
                      .HasForeignKey(ur => ur.UserId).IsRequired();

      _ = modelBuilder.Entity<AppRole>().HasMany(ur => ur.UserRoles).WithOne(u => u.Role)
                      .HasForeignKey(ur => ur.RoleId).IsRequired();

      _ = modelBuilder.Entity<UserLike>()
        .HasKey(k => new { k.SourceUserId, k.LikedUserId });
      _ = modelBuilder.Entity<UserLike>()
         .HasOne(s => s.SourceUser)
         .WithMany(l => l.LikedUsers)
         .HasForeignKey(s => s.SourceUserId)
         .OnDelete(DeleteBehavior.Cascade);
      _ = modelBuilder.Entity<UserLike>()
        .HasOne(s => s.LikedUser)
        .WithMany(l => l.LikedByUsers)
        .HasForeignKey(s => s.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);
      // Message Entity config
      _ = modelBuilder.Entity<Message>()
        .HasOne(u => u.Recipient)
        .WithMany(m => m.MessagesReceived)
        /* .HasForeignKey(u => u.RecipientId) */
        .OnDelete(DeleteBehavior.Restrict);
      _ = modelBuilder.Entity<Message>()
        .HasOne(u => u.Sender)
        .WithMany(m => m.MessagesSent)
        /* .HasForeignKey(u => u.SenderId) */
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}
