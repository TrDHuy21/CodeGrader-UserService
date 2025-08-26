using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using Application.Services.Interface;
using AutoMapper;
using Common;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Application.Services.Implement
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly USContext _uSContext;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, USContext uSContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uSContext = uSContext;
        }
        public async Task<Result<UserViewDto>> AddUser(UserCreateDto userCreateDto)
        {
            var errors = new List<ErrorField>();

            // Username validation
            if (string.IsNullOrEmpty(userCreateDto.Username))
            {
                errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username is required" });
            }
            else
            {
                if (userCreateDto.Username.Length < 3 || userCreateDto.Username.Length > 20)
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username must be 3-20 characters" });

                if (!Regex.IsMatch(userCreateDto.Username, @"^[a-zA-Z][A-Za-z0-9_]*$"))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username can only contain letters, numbers, and underscores" });

                if (await _uSContext.User.AnyAsync(u => u.Username == userCreateDto.Username))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username already exists" });
            }

            // Password validation
            if (string.IsNullOrEmpty(userCreateDto.Password))
            {
                errors.Add(new ErrorField { Field = "Password", ErrorMessage = "Password is required" });
            }
            else
            {
                if (!Regex.IsMatch(userCreateDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    errors.Add(new ErrorField { Field = "Password", ErrorMessage = "Password must have at least 8 characters, including uppercase, lowercase, number, and special character." });
                }
            }

            // Email validation
            if (string.IsNullOrEmpty(userCreateDto.Email))
            {
                errors.Add(new ErrorField { Field = "Email", ErrorMessage = "Email is required" });
            }
            else
            {
                if (!new EmailAddressAttribute().IsValid(userCreateDto.Email))
                    errors.Add(new ErrorField { Field = "Email", ErrorMessage = "Invalid email format. use (ngoc@example.com)" });

                if (await _uSContext.User.AnyAsync(u => u.Email == userCreateDto.Email))
                    errors.Add(new ErrorField { Field = "Email", ErrorMessage = "Email already exists" });
            }

            // FullName validation
            if (string.IsNullOrEmpty(userCreateDto.FullName))
            {
                errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "FullName is required" });
            }
            else
            {
                if (userCreateDto.FullName.Length < 3 || userCreateDto.FullName.Length > 50)
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "FullName must be 3-50 characters" });
            }

            // roleId validation
            var role = await _unitOfWork.RoleRepositories.GetByIdAsync(userCreateDto.RoleId);
            if (role == null)
            {
                errors.Add(new ErrorField { Field = "RoleId", ErrorMessage = "RoleId does not exist" });
            }

            // Nếu có lỗi, trả về chi tiết
            if (errors.Any())
                return Result<UserViewDto>.Failure(errors);

            // Map và lưu user
            var user = _mapper.Map<User>(userCreateDto);
            user.CreatedAt = DateTime.UtcNow;
            user.IsEmailConfirmed = true;
            try
            {             
                await _unitOfWork.UserRepositories.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<UserViewDto>.Failure("An error occurred while creating the user");
            }
           
            return Result<UserViewDto>.Success(null, "User created successfully");
        }
        public async Task<Result<string>> DeleteUser(int id)
        {
            var user = _unitOfWork.UserRepositories.GetByIdAsync(id);
            if (user == null)
            {
                return Result<string>.Failure("User not found");
            }
            await _unitOfWork.UserRepositories.DeleteAsync(user.Result);
            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success(null, "User deleted successfully");
        }
        public async Task<Result<IEnumerable<UserViewDto>>> GetAllUser()
        {
           var user =  await _unitOfWork.UserRepositories.GetAllAsync();
            var userDto = _mapper.Map<IEnumerable<UserViewDto>>(user);
            return Result<IEnumerable<UserViewDto>>.Success(userDto, "Get all user successfully");
        }
        public async Task<Result<UserViewDto>> GetUserById(int id)
        {
            var user = await _unitOfWork.UserRepositories.GetByIdAsync(id);
            if (user == null)
            {
                return Result<UserViewDto>.Failure("User not found");
            }
            var userViewDto = _mapper.Map<UserViewDto>(user);

            return Result<UserViewDto>.Success(userViewDto, "Get user by id successfully");
        }
        public async Task<Result<UserViewDto>> LockUser(int id)
        {
            var user = await _unitOfWork.UserRepositories.GetByIdAsync(id);
            if(user == null)
            {
                return Result<UserViewDto>.Failure("User not found");
            }
            user.IsActive = false;
            await _unitOfWork.UserRepositories.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var userDto = _mapper.Map<UserViewDto>(user);
            return Result<UserViewDto>.Success(userDto, "User locked successfully");
        }
        public async Task<Result<UserViewDto>> UnlockUser(int id)
        {
            var user = await _unitOfWork.UserRepositories.GetByIdAsync(id);
            if(user == null)
            {
                return Result<UserViewDto>.Failure("User not found");
            }
            user.IsActive = true;
            await _unitOfWork.UserRepositories.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserViewDto>(user);
            return Result<UserViewDto>.Success(userDto, "User uncloked successfully");
        }
        public async Task<Result<UserViewDto>> UpdateUser(UserUpdateByAdminDto dto)
        {
            var errors = new List<ErrorField>();

            var existingUser = await _unitOfWork.UserRepositories.GetByIdAsync(dto.Id);
            if (existingUser == null)
                return Result<UserViewDto>.Failure("User not found.");

            // Validate Username
            if (string.IsNullOrWhiteSpace(dto.Username))
                errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username is required" });
            else
            {
                if (dto.Username.Length < 3 || dto.Username.Length > 20)
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username must be 3-20 characters" });

                if (!Regex.IsMatch(dto.Username, @"^[a-zA-Z][A-Za-z0-9_]*$"))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username can only contain letters, numbers, and underscores" });

                if (await _uSContext.User.AnyAsync(u => u.Username == dto.Username && u.Id != dto.Id))
                    errors.Add(new ErrorField { Field = "Username", ErrorMessage = "Username already exists" });
            }

            // Validate FullName
            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name is required" });
            else
            {
                if (dto.FullName.Length < 3 || dto.FullName.Length > 50)
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name must be 3-50 characters" });
                if (!Regex.IsMatch(dto.FullName, @"^[a-zA-Z\s]+$"))
                    errors.Add(new ErrorField { Field = "FullName", ErrorMessage = "Full name can only contain letters and spaces" });
            }

            // Validate Birthday
            DateOnly? birthday = null;
            if (!string.IsNullOrEmpty(dto.Birthday))
            {
                if (!DateOnly.TryParse(dto.Birthday, out var parsedDate))
                    errors.Add(new ErrorField { Field = "Birthday", ErrorMessage = "Invalid birthday format. Use 'yyyy-MM-dd'." });
                else
                    birthday = parsedDate;
            }

            // Validate RoleId
            var role = await _unitOfWork.RoleRepositories.GetByIdAsync(dto.RoleId);
            if (role == null)
                errors.Add(new ErrorField { Field = "RoleId", ErrorMessage = "RoleId does not exist" });

            // validate Bio
            if (!string.IsNullOrEmpty(dto.Bio) && dto.Bio.Length > 500)
                errors.Add(new ErrorField { Field = "Bio", ErrorMessage = "Bio cannot exceed 500 characters" });

            // Validate URLs
            if (!string.IsNullOrEmpty(dto.GithubLink) && !Uri.IsWellFormedUriString(dto.GithubLink, UriKind.Absolute))
                errors.Add(new ErrorField { Field = "GithubLink", ErrorMessage = "Invalid URL format for GithubLink" });
            if (!string.IsNullOrEmpty(dto.LinkedInLink) && !Uri.IsWellFormedUriString(dto.LinkedInLink, UriKind.Absolute))
                errors.Add(new ErrorField { Field = "LinkedInLink", ErrorMessage = "Invalid URL format for LinkedInLink" });

            if (errors.Any())
                return Result<UserViewDto>.Failure(errors);

            // Update user
            existingUser.Username = dto.Username;
            existingUser.FullName = dto.FullName;
            existingUser.Birthday = birthday;
            existingUser.Bio = dto.Bio;
            existingUser.IsActive = dto.IsActive;
            existingUser.RoleId = dto.RoleId;
            existingUser.GithubLink = dto.GithubLink;
            existingUser.LinkedInLink = dto.LinkedInLink;

            var userDto = _mapper.Map<UserViewDto>(existingUser);

            try
            {
                await _unitOfWork.UserRepositories.UpdateAsync(existingUser);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<UserViewDto>.Failure("An error occurred while updating the user.");
            }


            return Result<UserViewDto>.Success(userDto, "User updated successfully.");
        }

    }
}
