using Domain.Entities;
using Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class USContext : DbContext
    {
        public USContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
             );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Avatar = "https://example.com/avatar1.png",
                    Username = "admin",
                    FullName = "Admin User",
                    Email = "admin@gmail.com ",
                    IsEmailConfirmed = true,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("Admin123_"),
                    CreatedAt = DateTime.UtcNow,
                    Birthday = new DateOnly(1900, 1, 1),
                    GithubLink = "",
                    LinkedInLink = "",
                    IsActive = true,
                    PasswordChangedAt = null,
                    Bio = "Admin user bio",
                    RoleId = 1                  
                },
                new User
                {
                    Id = 2,
                    Avatar = "https://example.com/avatar2.png",
                    Username = "user",
                    FullName = "Regular User",
                    Email = "user@gmail.com",
                    IsEmailConfirmed = true,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("User123_"),
                    CreatedAt = DateTime.UtcNow,
                    Birthday = new DateOnly(1900, 1, 1),
                    GithubLink = "",
                    LinkedInLink = "",
                    IsActive = true,
                    PasswordChangedAt = null,
                    Bio = "Regular user bio",
                    RoleId = 2
                }
             );

        }
    }
}
