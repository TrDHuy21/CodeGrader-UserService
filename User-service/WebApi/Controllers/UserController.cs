using Application.Dtos.UserDto;
using Application.Services.Implement;
using Application.Services.Interface;
using Common;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> Profile([FromRoute] string username)
        {
            var result = await _userService.GetProfileByUsername(username);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile/update-info")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto userUpdateDto)
        {
            var result = await _userService.UpdateUser(userUpdateDto);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var result = await _userService.ChangePassword(changePasswordDto);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("update-avatar")]
        public async Task<IActionResult> UpdateAvatar([FromForm] UpdateAvatarDto upDateAvatarDto)
        {
            var result = await _userService.UpdateAvatar(upDateAvatarDto);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("set-avatar-default")]
        public async Task<IActionResult> SetAvatarDefault()
        {
            var result = await _userService.SetAvatarDefault();
            return Ok(result);
        }
    }
}
