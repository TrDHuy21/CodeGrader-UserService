using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace Application.Services.Interface
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> Login(LoginDto loginDto);

        Task<Result<UserViewDto>> Register(UserCreateDto userCreateDto);
        
    }
}
