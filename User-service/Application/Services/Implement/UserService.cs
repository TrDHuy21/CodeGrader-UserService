using Application.Dtos.UserDto;
using Application.Services.Interface;
using AutoMapper;
using Common;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


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
            if(userCreateDto == null)
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
            var userDto = _mapper.Map<UserViewDto>(user);
            if(user == null)
            {
                return Result<UserViewDto>.Failure("User not found");
            }

            return Result<UserViewDto>.Success(userDto, null);
        }

        public Task<Result<User>> GetUserById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            if (userUpdateDto == null)
                {
                    return Result<User>.Failure("User data is required.");
                }

            // Check authorization (Unauthorized)     

            // Retrieve user from DB
            var existingUser = await _unitOfWork.UserRepositories.GetByIdAsync(userUpdateDto.Id);
            if (existingUser == null)
            {
                return Result<User>.Failure("User not found.");
            }

            // Check if Username is unique (if changed)
            if (userUpdateDto.Username != existingUser.Username &&
                await _uSContext.User.AnyAsync(u => u.Username == userUpdateDto.Username && u.Id != userUpdateDto.Id))
            {
                return Result<User>.Failure("Username already exists.");
            }

            // Check required fields
            if (string.IsNullOrWhiteSpace(userUpdateDto.Username))
            {
                return Result<User>.Failure("Username is required.");
            }
            if (string.IsNullOrWhiteSpace(userUpdateDto.FullName))
            {
                return Result<User>.Failure("Full name is required.");
            }

            // Map DTO to entity
            _mapper.Map(userUpdateDto, existingUser);

            // Ensure HashPassword is not overwritten to null
            if (string.IsNullOrWhiteSpace(existingUser.HashPassword))
            {
                return Result<User>.Failure("Password is required and cannot be empty.");
            }

            // Update and save
            try
            {
                await _unitOfWork.UserRepositories.UpdateAsync(existingUser);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE") ?? false)
                {
                    return Result<User>.Failure("Username already exists.");
                }
                return Result<User>.Failure($"Error updating user: {ex.InnerException?.Message}");
            }
            return Result<User>.Success(null, "User updated successfully.");
        }


    }

}
