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

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, USContext uSContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uSContext = uSContext;
        }

        public async Task<Result<UserViewDto>> AddUser(UserCreateDto userCreateDto)
        {
            if (userCreateDto == null)
            {
                return Result<UserViewDto>.Failure("Invalid user data");
            }

            var user = _mapper.Map<User>(userCreateDto);
            await _unitOfWork.UserRepositories.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<UserViewDto>.Success(null, "User created successfully");
        }

        public Task<Result<User>> DeleteUser(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<User>>> GetAllUser()
        {
            throw new NotImplementedException();
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
            return Result<UserViewDto>.Success(userDto, null);
        }

        public Task<Result<User>> GetUserById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var errors = new List<ErrorField>();


            if (userUpdateDto == null)
            {
                return Result<User>.Failure("User data is required.");
            }

            // chekck if user exists
            var existingUser = await _unitOfWork.UserRepositories.GetByIdAsync(userUpdateDto.Id);
            if (existingUser == null)
            {
                return Result<User>.Failure("User not found.");
            }

            // Check username 
            if (string.IsNullOrWhiteSpace(userUpdateDto.Username))
            {
                errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username is required" });
            }
            else
            {
                if (userUpdateDto.Username.Length < 3 || userUpdateDto.Username.Length > 20)
                {
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username must be 3 - 20 characters" });
                }
                if (!Regex.IsMatch(userUpdateDto.Username, @"^[a-zA-Z][A-Za-z0-9_]*$"))
                {
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username can only contain letters, numbers, and underscores" });
                }
                if (await _uSContext.User.AnyAsync(u => u.Username == userUpdateDto.Username && u.Id != userUpdateDto.Id))
                {
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username already exists" });
                }
            }

            // Check birthday
            if (!string.IsNullOrEmpty(userUpdateDto.Birthday))
            {
                if (!DateOnly.TryParse(userUpdateDto.Birthday, out var result))
                {
                    errors.Add(new ErrorField { Field = "Birthday", ErrorMessage = "Invalid birthday format. Use 'yyyy-MM-dd'." });
                }
            }

            // check fullname
            if (string.IsNullOrWhiteSpace(userUpdateDto.FullName))
            {
                errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name is required" });
            }
            else
            {
                if (userUpdateDto.FullName.Length < 3 || userUpdateDto.FullName.Length > 50)
                {
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "FullName must be 3-50 characters" });
                }
                if (!Regex.IsMatch(userUpdateDto.FullName, @"^[a-zA-Z\s]+$"))
                {
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name can only contain letters and spaces" });
                }
            }

            if (errors.Any())
            {
                return Result<User>.Failure(errors);
            }

            _mapper.Map(userUpdateDto, existingUser);

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

        public async Task<Result<User>> ChangePassword(int userId, ChangePasswordDto changePasswordDto)
        {
            var errors = new List<ErrorField>();

            // check userid 
            if (userId <= 0)
            {
                errors.Add(new ErrorField { Field = "UserId", ErrorMessage = "User ID is required" });
            }
            else
            {
                bool userExists = await _uSContext.User.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    errors.Add(new ErrorField { Field = "UserId", ErrorMessage = "User not found" });
                }
            }

            // check current password
            if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword))
            {
                errors.Add(new ErrorField { Field = "CurrentPassword", ErrorMessage = "CurrentPassword is required" });
            }
            else
            {
                var user = await _unitOfWork.UserRepositories.GetByIdAsync(userId);
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.HashPassword))
                {
                    errors.Add(new ErrorField { Field = "CurrentPassword", ErrorMessage = "CurrentPassword is not correct" });
                }
            }
            // check new password
            if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
            {
                errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "NewPassword is required" });
            }
            else
            {
                if (!Regex.IsMatch(changePasswordDto.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "Invalid NewPassword format" });
                }
            }
            if (errors.Any())
            {
                return Result<User>.Failure(errors);
            }
            return Result<User>.Success(null, "Change password successful");

        }
    }

}
