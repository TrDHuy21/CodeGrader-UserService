using Application.Dtos.UserDto;
using Common;
using Domain.Entities;

namespace Application.Services.Interface
{
    public interface IUserService 
    {
        Task<Result<User>> UpdateUser(UserUpdateDto userUpdateDto);
        Task<Result<UserViewDto>> GetProfileByUsername(string username);
        Task<Result<UserViewDto>> GetProfileByUserId(int UserId);
        Task<Result<User>> ChangePassword(ChangePasswordDto changePasswordDto);
        Task<Result<string>> UpdateAvatar(UpdateAvatarDto upDateAvatarDto);
        Task<Result<string>> SetAvatarDefault();
    }
}
