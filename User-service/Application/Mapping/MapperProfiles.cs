using Application.Dtos.UserDto;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<User, UserViewDto>()
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday.HasValue ? src.Birthday.Value.ToString("yyyy-MM-dd") : null));
            CreateMap<UserViewDto, User>();
                
            CreateMap<User, UserCreateDto>();
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.HashPassword,
                opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            CreateMap<UserUpdateDto, User>()
                .ForMember(x => x.Birthday, 
                opt => opt.MapFrom(x => string.IsNullOrEmpty(x.Birthday) ? (DateOnly?)null : DateOnly.ParseExact(x.Birthday, "yyyy-MM-dd")));
            CreateMap<User, UserUpdateDto>();

        }
    }
}
