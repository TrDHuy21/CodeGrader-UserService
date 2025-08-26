using Application.Dtos.UserDto;
using Application.Services.Interface;
using AutoMapper;
using Common;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace Application.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly USContext _uSContext;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, USContext uSContext, ICloudStorageService cloudStorageService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uSContext = uSContext;
            _cloudStorageService = cloudStorageService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<UserViewDto>> GetProfileByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Result<UserViewDto>.Failure("Username cannot be null or empty");
            }

            var user = await _unitOfWork.UserRepositories.GetProfileByUserName(username);
            if (user == null)
            {
                return Result<UserViewDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserViewDto>(user);
            return Result<UserViewDto>.Success(userDto, "Profile fetched successfully");
        }
        public async Task<Result<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("Id")?.Value;
            if (userIdClaim == null)
                return Result<User>.Failure("Invalid token");

            var userId = int.Parse(userIdClaim);

            var errors = new List<ErrorField>();

            // Lấy user hiện tại
            var existingUser = await _unitOfWork.UserRepositories.GetByIdAsync(userId);
            if (existingUser == null)
                return Result<User>.Failure("User not found.");

            // Validate Username
            if (string.IsNullOrWhiteSpace(userUpdateDto.Username))
            {
                errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username is required" });
            }
            else
            {
                if (userUpdateDto.Username.Length < 3 || userUpdateDto.Username.Length > 20)
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username must be 3 - 20 characters" });

                if (!Regex.IsMatch(userUpdateDto.Username, @"^[a-zA-Z][A-Za-z0-9_]*$"))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username can only contain letters, numbers, and underscores" });

                if (await _uSContext.User.AnyAsync(u => u.Username == userUpdateDto.Username && u.Id != userId))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username already exists" });
            }

            // Validate FullName
            if (string.IsNullOrWhiteSpace(userUpdateDto.FullName))
            {
                errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name is required" });
            }
            else
            {
                if (userUpdateDto.FullName.Length < 3 || userUpdateDto.FullName.Length > 50)
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name must be 3-50 characters" });

                if (!Regex.IsMatch(userUpdateDto.FullName, @"^[a-zA-Z\s]+$"))
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name can only contain letters and spaces" });
            }

            // Validate Birthday
            DateOnly? birthday = null;
            if (!string.IsNullOrEmpty(userUpdateDto.Birthday))
            {
                if (!DateOnly.TryParse(userUpdateDto.Birthday, out var parsed))
                {
                    errors.Add(new ErrorField { Field = "Birthday", ErrorMessage = "Invalid birthday format. Use 'yyyy-MM-dd'." });
                }
                else
                {
                    birthday = parsed;
                }
            }

            // Validate Bio
            if (!string.IsNullOrEmpty(userUpdateDto.Bio) && userUpdateDto.Bio.Length > 500)
            {
                errors.Add(new ErrorField { Field = "Bio", ErrorMessage = "Bio cannot exceed 500 characters" });
            }
            // Validate GithubLink
            if (!string.IsNullOrEmpty(userUpdateDto.GithubLink) && !Uri.IsWellFormedUriString(userUpdateDto.GithubLink, UriKind.Absolute))
            {
                errors.Add(new ErrorField { Field = "GithubLink", ErrorMessage = "Invalid GitHub link" });
            }
            // Validate LinkedInLink
            if (!string.IsNullOrEmpty(userUpdateDto.LinkedInLink) && !Uri.IsWellFormedUriString(userUpdateDto.LinkedInLink, UriKind.Absolute))
            {
                errors.Add(new ErrorField { Field = "LinkedInLink", ErrorMessage = "Invalid LinkedIn link" });
            }

            if (errors.Any())
                return Result<User>.Failure(errors);


            existingUser.Username = userUpdateDto.Username;
            existingUser.FullName = userUpdateDto.FullName;
            existingUser.Birthday = birthday;
            existingUser.Bio = userUpdateDto.Bio;
            existingUser.GithubLink = userUpdateDto.GithubLink;
            existingUser.LinkedInLink = userUpdateDto.LinkedInLink;

            try
            {
                await _unitOfWork.UserRepositories.UpdateAsync(existingUser);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<User>.Failure("An error occurred while updating the user.");
            }

            return Result<User>.Success(null, "User updated successfully.");
        }
        public async Task<Result<User>> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("Id")?.Value;
            if (userIdClaim == null)
            {
                return Result<User>.Failure("Invalid token");
            }

            var userId = int.Parse(userIdClaim);
            var errors = new List<ErrorField>();
            var user = await _unitOfWork.UserRepositories.GetByIdAsync(userId);

            if (user == null)
            {
                errors.Add(new ErrorField { Field = "UserId", ErrorMessage = "User not found" });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword))
                    errors.Add(new ErrorField { Field = "CurrentPassword", ErrorMessage = "CurrentPassword is required" });
                else if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.HashPassword))
                    errors.Add(new ErrorField { Field = "CurrentPassword", ErrorMessage = "CurrentPassword is not correct" });

                if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
                    errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "NewPassword is required" });
                else if (!Regex.IsMatch(changePasswordDto.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                    errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "Password must have at least 8 characters, including uppercase, lowercase, number, and special character." });
            }

            if (errors.Any())
                return Result<User>.Failure(errors);

            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;

            try
            {
                await _unitOfWork.UserRepositories.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                
                return Result<User>.Failure("An error occurred while changing the password.");
            }

            return Result<User>.Success(null, "Change password successful");
        }
        public async Task<Result<string>> UpdateAvatar(UpdateAvatarDto upDateAvatarDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("Id")?.Value;
            if (userIdClaim == null)
            {
                return Result<string>.Failure("Invalid token");

            }
            var userId = int.Parse(userIdClaim);

            var user = await _unitOfWork.UserRepositories.GetByIdAsync(userId);

            if(user == null)
            {
                return Result<string>.Failure("User not found");
            }
            if (upDateAvatarDto.Avatar == null || upDateAvatarDto.Avatar.Length == 0)
            {
                return Result<string>.Failure("Invalid avatar file");
            }
            var extension = Path.GetExtension(upDateAvatarDto.Avatar.FileName);
            var fileName = $"avatar_{user.Id}{extension}";

            var oldAvatar = user.Avatar;        

            var imageUrl = await _cloudStorageService.UploadFileAsync(upDateAvatarDto.Avatar, fileName);

            if (!imageUrl.IsSuccess)
            {
                return Result<string>.Failure($"Failed to upload avatar: {imageUrl.Data}");
            }
 
            user.Avatar = imageUrl.Data;
            await _unitOfWork.UserRepositories.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(imageUrl.Data, "Update avatar successful");
        }
        public async Task<Result<string>> SetAvatarDefault()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("Id")?.Value;
            if (userIdClaim == null)
            {
                return Result<string>.Failure("Invalid token");
            }
            var userId = int.Parse(userIdClaim);

            var user = await _unitOfWork.UserRepositories.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<string>.Failure("User not found");
            }
            var oldAvatar = user.Avatar;
            var defaultAvatarUrl = "https://res.cloudinary.com/dew9go5as/image/upload/v1756093480/default-avatar_rxyvxb.png";
        
            user.Avatar = defaultAvatarUrl;

            await _unitOfWork.UserRepositories.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrEmpty(oldAvatar) && oldAvatar != defaultAvatarUrl)
            {
                await _cloudStorageService.DeleteFileAsync(oldAvatar);
            }

            return Result<string>.Success(defaultAvatarUrl, "Set avatar to default successful");
        }
    }

}
