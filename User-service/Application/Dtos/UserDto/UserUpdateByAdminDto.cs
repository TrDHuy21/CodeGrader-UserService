namespace Application.Dtos.UserDto
{
    public class UserUpdateByAdminDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string? Birthday { get; set; }
        public string? Bio { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public string? GithubLink { get; set; }
        public string? LinkedInLink { get; set; }
    }
}
