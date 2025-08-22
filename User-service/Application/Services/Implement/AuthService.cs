using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using Application.Services.Interface;
using AutoMapper;
using Common;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly USContext _uSContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(IConfiguration configuration, USContext uSContext, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _configuration = configuration;
            _uSContext = uSContext;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<LoginResponse>> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.UserNameOrEmail) || string.IsNullOrEmpty(loginDto.Password))
            {
                return  Result<LoginResponse>.Failure("Username or password cannot be null or empty");
            }
            var user = _uSContext.User
                .FirstOrDefault(u => (u.Username == loginDto.UserNameOrEmail || u.HashPassword == loginDto.Password)
                 && u.HashPassword == loginDto.Password);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid username or password");
            }
            var roleName = _uSContext.Role
                .Where(ur => ur.Id == user.RoleId)
                .Select(ur => ur.Name)
                .FirstOrDefault();

            // congig token
            var tokenHandler = new JwtSecurityTokenHandler();

            // key
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // token descriptor

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim("Username", user.Username),
                    new Claim("Role", roleName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Audience = _configuration["Jwt:Audience"],
                Issuer = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Result<LoginResponse>.Success(new LoginResponse
            {
                UserDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    RoleName = roleName,
                    CreatedAt = user.CreatedAt
                },
                tokenDto = new TokenDto
                {
                    AccessToken = tokenString
                }
            }, "Login successful");
        }

        public async Task<Result<UserViewDto>> Register(UserCreateDto userCreateDto)
        {
            if (userCreateDto == null)
                return Result<UserViewDto>.Failure("User data is required");

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
                    errors.Add(new ErrorField { Field = "Password", ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character" });
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

            // Nếu có lỗi, trả về chi tiết
            if (errors.Any())
                return Result<UserViewDto>.Failure(errors);

            // Map và lưu user
            try
            {
                var user = _mapper.Map<User>(userCreateDto);
                user.CreatedAt = DateTime.Now;
                await _unitOfWork.UserRepositories.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<UserViewDto>.Failure("An error occurred while registering the user");
            }

            return Result<UserViewDto>.Success(null, "Register successful");
        }
    }
}
