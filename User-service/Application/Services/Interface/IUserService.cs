using Application.Dtos.UserDto;
using Common;
using Domain.Entities;

namespace Application.Services.Interface
{
    public interface IUserService 
    {
        Task<Result<IEnumerable<User>>> GetAllUser();
        Task<Result<User>> GetUserById(int id);
        Task<Result<UserViewDto>> AddUser(UserCreateDto user);
        Task<Result<User>> UpdateUser(UserUpdateDto userUpdateDto);
        Task<Result<User>> DeleteUser(int id);
        Task<Result<UserViewDto>> GetProfileByUsername(string username);
        Task<Result<User>> ChangePassword(int userId, ChangePasswordDto changePasswordDto);
    }
}
