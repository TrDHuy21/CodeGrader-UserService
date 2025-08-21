using Application.Dtos.UserDto;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<User, UserViewDto>().ReverseMap();
            CreateMap<User, UserCreateDto>();
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.HashPassword,
                opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(dest => dest.Birthday,
                opt => opt.MapFrom(src => DateOnly.ParseExact(src.Birthday, "yyyy-MM-dd")));

            CreateMap<UserUpdateDto, User>()
                .ForMember(x => x.Birthday, 
                opt => opt.MapFrom(x => DateOnly.ParseExact(x.Birthday, "yyyy-MM-dd")));
            CreateMap<User, UserUpdateDto>();
        }
    }
}
