using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshToken");
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Id).ValueGeneratedOnAdd();

            builder.Property(rt => rt.Token).IsRequired().HasMaxLength(500);

            builder.Property(rt => rt.Expires).IsRequired();

            builder.Property(rt => rt.Created).IsRequired();

            builder.Property(rt => rt.Revoked)
                .IsRequired(false);
                
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
