using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using Application.Services.Implement;
using Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {       
            var result = await _authService.Login(loginDto);
            return Ok(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> FogotPassword(FogotPasswordDto fogotPasswordDto)
        {
            var result = await _authService.ForgotPassword(fogotPasswordDto);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> VerifyOtpAndResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.VerifyOtpAndResetPassword(resetPasswordDto);
            return Ok(result);
        }
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            var result = await _authService.ConfirmEmail(confirmEmailDto);
            return Ok(result);
        }

        [HttpPost("send-otp-email")]
        public async Task<IActionResult> SendOtpEmail(FogotPasswordDto fogotPasswordDto)
        {
            var result = await _authService.SendOtpEmail(fogotPasswordDto);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshToken(refreshTokenDto.Token);
            return Ok(result);
        }

        [HttpPut("logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var result = await _authService.LogOut(refreshToken);
            return Ok(result);
        }
    }
}
