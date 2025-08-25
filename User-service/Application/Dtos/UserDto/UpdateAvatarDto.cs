using Microsoft.AspNetCore.Http;

namespace Application.Dtos.UserDto
{
    public class UpdateAvatarDto
    {
        public IFormFile Avatar { get; set; }
    }
}
