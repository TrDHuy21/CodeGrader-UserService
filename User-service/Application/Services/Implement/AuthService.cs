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
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;

        public AuthService(IConfiguration configuration, USContext uSContext, IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache memoryCache, IEmailService emailService)
        {
            _configuration = configuration;
            _uSContext = uSContext;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _emailService = emailService;
        }
        public async Task<Result<LoginResponse>> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.UserNameOrEmail) || string.IsNullOrEmpty(loginDto.Password))
            {
                return  Result<LoginResponse>.Failure("Username or password cannot be null or empty");
            }
            var user = _uSContext.User
                .FirstOrDefault(u => (u.Username == loginDto.UserNameOrEmail || u.Email == loginDto.UserNameOrEmail));

            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid username or password");
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashPassword);

            if (!isValidPassword)
            {
                return Result<LoginResponse>.Failure("Invalid username or password");

            }
            if (!user.IsEmailConfirmed)
            {
                return Result<LoginResponse>.Failure("Please confirm your email before logging in");
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

            // Nếu có lỗi, trả về chi tiết
            if (errors.Any())
                return Result<UserViewDto>.Failure(errors);

            // Map và lưu user
            try
            {
                var user = _mapper.Map<User>(userCreateDto);
                user.CreatedAt = DateTime.Now;
                user.IsEmailConfirmed = false;

                await _unitOfWork.UserRepositories.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<UserViewDto>.Failure("An error occurred while registering the user");
            }
            // Send verification email
            var otp = new Random().Next(100000,999999).ToString();
            _memoryCache.Set(userCreateDto.Email, otp, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(userCreateDto.Email, "Verify your email", $"Your OTP code is: <b>{otp}</b>. It will expire in 10 minutes");

            return Result<UserViewDto>.Success(null, "Register successful! Please check your email to verify your account.");
        }
        public async Task<Result<string>> ForgotPassword(FogotPasswordDto fogotPasswordDto)
        {
            var user = await _uSContext.User.FirstOrDefaultAsync(u => u.Email == fogotPasswordDto.Email);
            if (user == null)
            {
                return Result<string>.Failure("Email not found");
            }

            var otp = new Random().Next(100000, 999999).ToString();

            _memoryCache.Set(fogotPasswordDto.Email, otp, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(fogotPasswordDto.Email, "Password Reset Code", $"Your OTP code is: <b>{otp}</b>. It will expire in 10 minutes");

            return Result<string>.Success(null, "OTP has been sent to your email");
        }
        public async Task<Result<string>> VerifyOtpAndResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var errors = new List<ErrorField>();

            // check email
            var userEmail = _uSContext.User.FirstOrDefault(u => u.Email == resetPasswordDto.Email);
            if (userEmail == null)
            {
                errors.Add(new ErrorField { Field = "Email", ErrorMessage = "Email not found" });
            }
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Email))
            {
                errors.Add(new ErrorField { Field = "Email", ErrorMessage = " Email is required" });
            }
          
            // check otp
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Otp))
            {
                errors.Add(new ErrorField { Field = "OTP", ErrorMessage = "OTP is required" });
            }
            else
            {
                if (!_memoryCache.TryGetValue(resetPasswordDto.Email, out string cachedOtp) || cachedOtp != resetPasswordDto.Otp)
                {
                    errors.Add(new ErrorField { Field = "OTP", ErrorMessage = "Invalid or expired OTP" });
                }
            }
            // check password
            if (string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword))
            {
                errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "NewPassword is required" });
            }
            else
            {
                if (!Regex.IsMatch(resetPasswordDto.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    errors.Add(new ErrorField { Field = "NewPassword", ErrorMessage = "NewPassword must have at least 8 characters, including uppercase, lowercase, number, and special character." });
                }          
            }

            if (errors.Any())
            {
                return Result<string>.Failure(errors);
            }

            var user = await _uSContext.User.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            if (user == null)
            {
                return Result<string>.Failure("Email not found");
            }
            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            await _unitOfWork.UserRepositories.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _memoryCache.Remove(resetPasswordDto.Email);

            return Result<string>.Success(null, "Password has been reset successfully");
        }
        public async Task<Result<string>> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _uSContext.User.FirstOrDefaultAsync(us => us.Email == confirmEmailDto.Email);
            if (user == null)
            {
                return Result<string>.Failure("Email not found");
            }
            else
            {
                if (user.IsEmailConfirmed)
                {
                    return Result<string>.Failure("Email is already confirmed");
                }
            }

            if (string.IsNullOrWhiteSpace(confirmEmailDto.Otp))
            {
                return Result<string>.Failure("OTP is required");
            }
            else
            {
                if (!_memoryCache.TryGetValue(confirmEmailDto.Email, out string cachedOtp) || cachedOtp != confirmEmailDto.Otp)
                {
                    return Result<string>.Failure("Invalid or expired OTP");
                }
            }
                user.IsEmailConfirmed = true;

            try
            {
                await _unitOfWork.UserRepositories.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Result<string>.Failure("An error occurred while confirming the email");
            }
            _memoryCache.Remove(confirmEmailDto.Email);
            return Result<string>.Success(null, "Email confirmed successfully");
        }
        public async Task<Result<string>> SendOtpEmail(FogotPasswordDto fogotPasswordDto)
        {
            var user = await _uSContext.User.FirstOrDefaultAsync(u => u.Email == fogotPasswordDto.Email);
            if (user == null)
            {
                return Result<string>.Failure("Email not found");
            }
            var otp = new Random().Next(100000,999999).ToString();

            _memoryCache.Set(fogotPasswordDto.Email, otp, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(fogotPasswordDto.Email, "Verify your email", $"Your OTP code is: <b>{otp}</b>. It will expire in 10 minutes");

            return Result<string>.Success(null, "OTP has been sent to your email");
        }
    }
}
