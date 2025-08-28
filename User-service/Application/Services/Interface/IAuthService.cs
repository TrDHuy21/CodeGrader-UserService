using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using Common;

namespace Application.Services.Interface
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> Login(LoginDto loginDto);
        Task<Result<UserViewDto>> Register(RegisterDto registerDt);
        Task<Result<string>> ForgotPassword(FogotPasswordDto fogotPasswordDto);
        Task<Result<string>> VerifyOtpAndResetPassword(ResetPasswordDto resetPasswordDto);
        Task<Result<string>> ConfirmEmail(ConfirmEmailDto confirmEmailDto);
        Task<Result<string>> SendOtpEmail(FogotPasswordDto fogotPasswordDto);
        Task<Result<TokenDto>> RefreshToken(string refreshToken);
        Task<Result<string>> LogOut(string refreshToken);
    }
}
