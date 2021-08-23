using System.Linq;
using API.DTO;
using API.Entities;
using API.Extensions;
using AutoMapper;
namespace API.Helpers
{
  public class AutoMappperProfiles : Profile
  {
    public AutoMappperProfiles()
    {
      _ = CreateMap<AppUser, MemberDto>()
        .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(photos => photos.IsMain).Url))
        .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
      _ = CreateMap<Photo, PhotoDto>();
      _ = CreateMap<MemberUpdateDto, AppUser>();
      _ = CreateMap<RegisterDto, AppUser>();
    }
  }
}
