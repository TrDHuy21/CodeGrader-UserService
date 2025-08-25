using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Avatar)
                .HasMaxLength(200);

            builder.Property(u => u.Username)
                .IsRequired()               
                .HasMaxLength(50);
            builder.HasIndex(u => u.Username)
                .IsUnique();
                
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.IsEmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.HashPassword)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.GithubLink)
                .HasMaxLength(200);

            builder.Property(u => u.LinkedInLink)
                .HasMaxLength(200);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.Bio)
                .HasMaxLength(500);

            builder.Property(u => u.RoleId).HasDefaultValue(2);
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
        }
    }
}
