using Application.Dtos.AuthDto;
using Application.Dtos.UserDto;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            // Entity -> View DTO
            CreateMap<User, UserViewDto>()
                .ForMember(dest => dest.Birthday,
                    opt => opt.MapFrom(src => src.Birthday.HasValue ? src.Birthday.Value.ToString("yyyy-MM-dd") : null));

            // User -> Update DTO (trả về dữ liệu cho client)
            CreateMap<User, UserUpdateDto>()
                .ForMember(dest => dest.Birthday,
                    opt => opt.MapFrom(src => src.Birthday.HasValue ? src.Birthday.Value.ToString("yyyy-MM-dd") : null));

            // Create User
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.HashPassword,
                    opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            // Register User
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.HashPassword,
                    opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            // User -> DTO trả về (Register/Create)
            CreateMap<User, RegisterDto>();


        }
    }
}
