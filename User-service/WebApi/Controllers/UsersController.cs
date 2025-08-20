using Application.Dtos.UserDto;
using Application.Services.Interface;
using Common;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> Profile(string username)
        {
            var result = await _userService.GetProfileByUsername(username);
            return Ok(result);
        }
        [Authorize]
        [HttpPut("profile/update-info")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto userUpdateDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Unauthorized();
            }
            var result = await _userService.UpdateUser(userUpdateDto);
            return Ok(result);
        }

    }
}
