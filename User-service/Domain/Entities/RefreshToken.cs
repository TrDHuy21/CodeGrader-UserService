namespace Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public User User { get; set; }
    }
}
