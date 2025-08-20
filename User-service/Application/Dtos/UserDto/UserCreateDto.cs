namespace Application.Dtos.UserDto
{
    public class UserCreateDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateOnly Birthday { get; set; }
        public string FullName { get; set; }
        public string? Bio { get; set; }
        public string? GithubLink { get; set; }
        public string? LinkedInLink { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
