using Application.Dtos.UserDto;
using Common;
using Domain.Entities;

namespace Application.Services.Interface
{
    public interface IAdminService
    {
        Task<Result<IEnumerable<UserViewDto>>> GetAllUser();
        Task<Result<UserViewDto>> GetUserById(int id);
        Task<Result<UserViewDto>> AddUser(UserCreateDto user);
        Task<Result<UserViewDto>> UpdateUser(UserUpdateByAdminDto userUpdateByAdminDto);
        Task<Result<string>> DeleteUser(int id);
        Task<Result<UserViewDto>> LockUser(int id);
        Task<Result<UserViewDto>> UnlockUser(int id);
    }
}
