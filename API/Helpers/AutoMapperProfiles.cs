using System.Linq;
using API.DTO;
using API.Entities;
using AutoMapper;
using API.Extensions;
namespace API.Helpers
{
    public class AutoMappperProfiles : Profile
    {
        public AutoMappperProfiles()
        {
          CreateMap<AppUser, MemberDto>()
            .ForMember(dest=>dest.PhotoUrl,opt=>opt.MapFrom(src =>src.Photos.FirstOrDefault(photos => photos.IsMain).Url))
            .ForMember(dest=>dest.Age,opt=>opt.MapFrom(src =>src.DateOfBirth.CalculateAge()));
          CreateMap<Photo,PhotoDto>();
        }
    }
}
