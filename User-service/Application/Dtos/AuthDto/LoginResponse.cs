namespace Application.Dtos.AuthDto
{
    public class LoginResponse
    {
      public UserDto UserDto { get; set; }
      public TokenDto tokenDto { get; set; }
    }
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

}
