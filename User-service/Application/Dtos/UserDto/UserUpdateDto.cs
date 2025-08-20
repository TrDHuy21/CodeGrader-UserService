namespace Application.Dtos.UserDto
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateOnly Birthday { get; set; }
        public string FullName { get; set; }
        public string? Bio { get; set; }
        public string? GithubLink { get; set; }
        public string? LinkedInLink { get; set; }
    }
}
