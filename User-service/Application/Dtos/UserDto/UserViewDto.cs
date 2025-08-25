namespace Application.Dtos.UserDto
{
    public class UserViewDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Birthday { get; set; }
        public string FullName { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string GithubLink { get; set; }
        public string LinkedinLink { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
