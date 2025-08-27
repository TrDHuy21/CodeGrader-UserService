using Application.Dtos.UserDto;
using Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _adminService.GetAllUser();
            return Ok(result);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _adminService.GetUserById(id);
            return Ok(result);
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] UserCreateDto user)
        {
            var result = await _adminService.AddUser(user);
            return Ok(result);
        }

        [HttpPut("user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateByAdminDto userUpdateByAdminDto)
        {
            var result = await _adminService.UpdateUser(userUpdateByAdminDto);
            return Ok(result);
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUser(id);
            return Ok(result);
        }

        [HttpPut("user/lock/{id}")]
        public async Task<IActionResult> LockUser(int id)
        {
            var result = await _adminService.LockUser(id);
            return Ok(result);
        }

        [HttpPut("user/unlock/{id}")]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var result = await _adminService.UnlockUser(id);
            return Ok(result);
        }
    }
}
