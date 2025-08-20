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
            {
                return Result<UserViewDto>.Failure("User data is required");
            }

            var errors = new List<string>();

            // Username validation
            if (string.IsNullOrWhiteSpace(userCreateDto.Username) ||
                userCreateDto.Username.Length < 3 || userCreateDto.Username.Length > 20 ||
                !Regex.IsMatch(userCreateDto.Username, @"^[A-Za-z][A-Za-z0-9_]*$"))
            {
                errors.Add("Username must be 3-20 characters, start with a letter, and contain only letters, numbers, or underscores");
            }
            else
            {
                var existingUser = await _uSContext.User.FirstOrDefaultAsync(u => u.Username == userCreateDto.Username);
                if (existingUser != null)
                {
                    errors.Add("Username already exists");
                }
            }

            // Password validation
            if (!Regex.IsMatch(userCreateDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
            {
                errors.Add("Password must be at least 8 characters and include uppercase, lowercase, number, and special character");
            }

            // Email validation
            if (!new EmailAddressAttribute().IsValid(userCreateDto.Email))
            {
                errors.Add("Invalid email format");
            }
            else
            {
                var disposableDomains = new List<string> { "mailinator.com", "tempmail.com" };
                var domain = userCreateDto.Email.Split('@')[1];
                if (disposableDomains.Contains(domain))
                {
                    errors.Add("Disposable emails are not allowed");
                }

                var existingEmail = await _uSContext.User.FirstOrDefaultAsync(u => u.Email == userCreateDto.Email);
                if (existingEmail != null)
                {
                    errors.Add("Email already exists");
                }
            }

            // FullName validation
            if (!string.IsNullOrEmpty(userCreateDto.FullName) && userCreateDto.FullName.Length > 50)
            {
                errors.Add("Full name must be at most 50 characters");
            }

            // Bio validation
            if (!string.IsNullOrEmpty(userCreateDto.Bio) && userCreateDto.Bio.Length > 200)
            {
                errors.Add("Bio must be at most 200 characters");
            }

            // LinkedIn link validation
            if (!string.IsNullOrEmpty(userCreateDto.LinkedInLink))
            {
                if (!Uri.TryCreate(userCreateDto.LinkedInLink, UriKind.Absolute, out var uri) || !uri.Host.Contains("linkedin.com"))
                {
                    errors.Add("LinkedIn link must be a valid linkedin.com URL");
                }
            }

            // Nếu có lỗi, trả về luôn
            if (errors.Any())
            {
                return Result<UserViewDto>.Failure(string.Join("; ", errors));
            }

            // Map và lưu user
            var user = _mapper.Map<User>(userCreateDto);

            await _unitOfWork.UserRepositories.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userViewDto = _mapper.Map<UserViewDto>(user);
            return Result<UserViewDto>.Success(userViewDto, "Register successful");
        }



    }
}
