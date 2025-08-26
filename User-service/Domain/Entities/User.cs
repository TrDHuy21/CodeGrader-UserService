namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HashPassword { get; set; }    
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? GithubLink { get; set; }
        public string? LinkedInLink { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Bio { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
