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
                .ForMember(dest => dest.HashPassword, opt =>
                opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            CreateMap<UserUpdateDto, User>().ReverseMap();
        }
    }
}
