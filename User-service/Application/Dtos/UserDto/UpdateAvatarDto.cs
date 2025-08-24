using Microsoft.AspNetCore.Http;

namespace Application.Dtos.UserDto
{
    public class UpdateAvatarDto
    {
        public int UserId { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
